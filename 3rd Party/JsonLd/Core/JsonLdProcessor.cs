using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonLD.Impl;
using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    /// <summary>
    ///     http://json-ld.org/spec/latest/json-ld-api/#the-jsonldprocessor-interface
    /// </summary>
    /// <author>tristan</author>
    public class JsonLdProcessor
    {
        /// <summary>
        ///     a registry for RDF Parsers (in this case, JSONLDSerializers) used by
        ///     fromRDF if no specific serializer is specified and options.format is set.
        /// </summary>
        /// <remarks>
        ///     a registry for RDF Parsers (in this case, JSONLDSerializers) used by
        ///     fromRDF if no specific serializer is specified and options.format is set.
        ///     TODO: this would fit better in the document loader class
        /// </remarks>
        private static readonly IDictionary<string, IRdfParser> rdfParsers = new RdfParserDictionary();

        /// <exception cref="JsonLdError"></exception>
        public static async Task<JObject> CompactAsync(JToken input,
                                                       JToken context,
                                                       JsonLdOptions opts)
        {
            // 1)
            // TODO: look into java futures/promises
            // 2-6) NOTE: these are all the same steps as in expand
            JToken expanded = await ExpandAsync(input, opts);

            // 7)
            if (context is JObject jObj && jObj.ContainsKey("@context"))
                context = jObj["@context"];

            var activeCtx = await Context.ParseAsync(context, opts);

            // 8)
            var compacted = new JsonLdApi(opts).Compact(activeCtx,
                                                        null,
                                                        expanded,
                                                        opts.GetCompactArrays
                                                            ());

            // final step of Compaction Algorithm
            // TODO: SPEC: the result result is a NON EMPTY array,
            if (compacted is JArray jArray)
                compacted = jArray.IsEmpty() ? new JObject() : new JObject {[activeCtx.CompactIri("@graph", true)] = compacted};

            if (!compacted.IsNull() && !context.IsNull())
                if (context is JObject && !((JObject)context).IsEmpty()
                    || context is JArray && !((JArray)context).IsEmpty())
                    compacted["@context"] = context;

            // 9)
            return (JObject)compacted;
        }

        /// <exception cref="JsonLdError"></exception>
        public static async Task<JArray> ExpandAsync(JToken input, JsonLdOptions opts)
        {
            // 1)
            // TODO: look into java futures/promises

            // 2) verification of DOMString IRI
            var isIriString = input.Type == JTokenType.String;
            if (isIriString)
            {
                var hasColon = false;
                foreach (var c in (string)input)
                {
                    if (c == ':') hasColon = true;

                    if (!hasColon && (c == '{' || c == '['))
                    {
                        isIriString = false;
                        break;
                    }
                }
            }

            if (isIriString)
            {
                try
                {
                    var tmp = await opts.documentLoader.LoadDocumentAsync((string)input);
                    input = tmp.document;
                }
                catch (Exception e)
                {
                    // TODO: figure out how to deal with remote context
                    throw new JsonLdError(JsonLdError.Error.LoadingDocumentFailed, e.Message);
                }

                // if set the base in options should override the base iri in the
                // active context
                // thus only set this as the base iri if it's not already set in
                // options
                if (opts.GetBase() == null) opts.SetBase((string)input);
            }

            // 3)
            var activeCtx = new Context(opts);

            // 4)
            if (opts.GetExpandContext() != null)
            {
                var exCtx = opts.GetExpandContext();
                if (exCtx is JObject && ((IDictionary<string, JToken>)exCtx).ContainsKey("@context"
                    ))
                    exCtx = (JObject)((IDictionary<string, JToken>)exCtx)["@context"];
                activeCtx = await activeCtx.ParseAsync(exCtx);
            }

            // 5)
            // TODO: add support for getting a context from HTTP when content-type
            // is set to a jsonld compatable format
            // 6)
            var expanded = await new JsonLdApi(opts).ExpandAsync(activeCtx, input);

            // final step of Expansion Algorithm
            if (expanded is JObject && ((IDictionary<string, JToken>)expanded).ContainsKey("@graph") && (
                                                                                                            (IDictionary<string, JToken>)expanded)
                .Count == 1)
            {
                expanded = ((JObject)expanded)["@graph"];
            }
            else
            {
                if (expanded.IsNull()) expanded = new JArray();
            }

            // normalize to an array
            if (!(expanded is JArray))
            {
                var tmp = new JArray();
                tmp.Add(expanded);
                expanded = tmp;
            }

            return (JArray)expanded;
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<JArray> ExpandAsync(JToken input)
        {
            return ExpandAsync(input, new JsonLdOptions(string.Empty));
        }

        /// <exception cref="JsonLdError"></exception>
        public static async Task<JToken> FlattenAsync(JToken input, JToken context, JsonLdOptions opts)
        {
            // 2-6) NOTE: these are all the same steps as in expand
            var expanded = await ExpandAsync(input, opts);

            // 7)
            if (context is JObject && ((IDictionary<string, JToken>)context).ContainsKey(
                    "@context"))
                context = context["@context"];

            // 8) NOTE: blank node generation variables are members of JsonLdApi
            // 9) NOTE: the next block is the Flattening Algorithm described in
            // http://json-ld.org/spec/latest/json-ld-api/#flattening-algorithm
            // 1)
            var nodeMap = new JObject();
            nodeMap["@default"] = new JObject();

            // 2)
            new JsonLdApi(opts).GenerateNodeMap(expanded, nodeMap);

            // 3)
            var defaultGraph = (JObject)Collections.Remove(nodeMap, "@default");

            // 4)
            foreach (var graphName in nodeMap.GetKeys())
            {
                var graph = (JObject)nodeMap[graphName];

                // 4.1+4.2)
                JObject entry;
                if (!defaultGraph.ContainsKey(graphName))
                {
                    entry = new JObject();
                    entry["@id"] = graphName;
                    defaultGraph[graphName] = entry;
                }
                else
                {
                    entry = (JObject)defaultGraph[graphName];
                }

                // 4.3)
                // TODO: SPEC doesn't specify that this should only be added if it
                // doesn't exists
                if (!entry.ContainsKey("@graph")) entry["@graph"] = new JArray();
                var keys = new JArray(graph.GetKeys());
                keys.SortInPlace();
                foreach (string id in keys)
                {
                    var node = (JObject)graph[id];
                    if (!(node.ContainsKey("@id") && node.Count == 1)) ((JArray)entry["@graph"]).Add(node);
                }
            }

            // 5)
            var flattened = new JArray();

            // 6)
            var keysArray = new JArray(defaultGraph.GetKeys());
            keysArray.SortInPlace();
            foreach (string id in keysArray)
            {
                var node = (JObject)defaultGraph[id];
                if (!(node.ContainsKey("@id") && node.Count == 1)) flattened.Add(node);
            }

            // 8)
            if (!context.IsNull() && !flattened.IsEmpty())
            {
                var activeCtx = new Context(opts);
                activeCtx = await activeCtx.ParseAsync(context);

                // TODO: only instantiate one jsonldapi
                var compacted = new JsonLdApi(opts).Compact(activeCtx,
                                                            null,
                                                            flattened,
                                                            opts.GetCompactArrays
                                                                ());
                if (!(compacted is JArray))
                {
                    compacted = new JArray { compacted };
                }

                var alias = activeCtx.CompactIri("@graph");
                var rval = activeCtx.Serialize();
                rval[alias] = compacted;
                return rval;
            }

            return flattened;
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<JToken> FlattenAsync(JToken input, JsonLdOptions opts)
        {
            return FlattenAsync(input, null, opts);
        }

        /// <exception cref="JsonLdError"></exception>
        public static async Task<JObject> FrameAsync(JToken input,
                                    JToken frame,
                                    JsonLdOptions options)
        {
            if (frame is JObject) frame = JsonLdUtils.Clone((JObject)frame);

            // TODO string/IO input
            JToken expandedInput = await ExpandAsync(input, options);
            var expandedFrame = await ExpandAsync(frame, options);
            var api = await JsonLdApi.CreateAsync(expandedInput, options);
            var framed = api.Frame(expandedInput, expandedFrame);
            var activeCtx = await api.context.ParseAsync(frame["@context"
                                              ]);
            var compacted = api.Compact(activeCtx, null, framed);
            if (!(compacted is JArray))
            {
                compacted = new JArray {compacted};
            }

            var alias = activeCtx.CompactIri("@graph");
            var rval = activeCtx.Serialize();
            rval[alias] = compacted;
            JsonLdUtils.RemovePreserve(activeCtx, rval, options);
            return rval;
        }

        public static void RegisterRdfParser(string format, IRdfParser parser)
        {
            rdfParsers[format] = parser;
        }

        public static void RemoveRdfParser(string format)
        {
            Collections.Remove(rdfParsers, format);
        }

        /// <summary>Converts an RDF dataset to JSON-LD.</summary>
        /// <remarks>Converts an RDF dataset to JSON-LD.</remarks>
        /// <param name="dataset">
        ///     a serialized string of RDF in a format specified by the format
        ///     option or an RDF dataset to convert.
        /// </param>
        /// <param name="options"></param>
        /// <exception cref="JsonLdError"></exception>
        public static async Task<JToken> FromRdfAsync(JToken dataset, JsonLdOptions options)
        {
            // handle non specified serializer case
            IRdfParser parser;
            if (options.format == null && dataset.Type == JTokenType.String) options.format = "application/nquads";
            if (rdfParsers.ContainsKey(options.format))
                parser = rdfParsers[options.format];
            else
                throw new JsonLdError(JsonLdError.Error.UnknownFormat, options.format);

            // convert from RDF
            return await FromRdfAsync(dataset, options, parser);
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<JToken> FromRdfAsync(JToken dataset)
        {
            return FromRdfAsync(dataset, new JsonLdOptions(string.Empty));
        }

        /// <summary>Uses a specific serializer.</summary>
        /// <remarks>Uses a specific serializer.</remarks>
        /// <exception cref="JsonLdError"></exception>
        public static async Task<JToken> FromRdfAsync(JToken input,
                                     JsonLdOptions options,
                                     IRdfParser parser
        )
        {
            var dataset = parser.Parse(input);

            // convert from RDF
            JToken rval = new JsonLdApi(options).FromRdf(dataset);

            // re-process using the generated context if outputForm is set
            if (options.outputForm != null)
                if ("expanded".Equals(options.outputForm))
                {
                    return rval;
                }
                else
                {
                    if ("compacted".Equals(options.outputForm)) return await CompactAsync(rval, dataset.GetContext(), options);

                    if ("flattened".Equals(options.outputForm))
                        return await FlattenAsync(rval, dataset.GetContext(), options);
                    throw new JsonLdError(JsonLdError.Error.UnknownError);
                }

            return rval;
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<JToken> FromRdfAsync(JToken input, IRdfParser parser)
        {
            return FromRdfAsync(input, new JsonLdOptions(string.Empty), parser);
        }

        /// <summary>Outputs the RDF dataset found in the given JSON-LD object.</summary>
        /// <remarks>Outputs the RDF dataset found in the given JSON-LD object.</remarks>
        /// <param name="input">the JSON-LD input.</param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        /// <exception cref="JsonLdError"></exception>
        public static async Task<object> ToRdfAsync(JToken input,
                                   IJsonLdTripleCallback callback,
                                   JsonLdOptions options)
        {
            JToken expandedInput = await ExpandAsync(input, options);
            var api = await JsonLdApi.CreateAsync(expandedInput, options);
            var dataset = api.ToRdf();

            // generate namespaces from context
            if (options.useNamespaces)
            {
                JArray _input;
                if (input is JArray array)
                {
                    _input = array;
                }
                else
                {
                    _input = new JArray();
                    _input.Add((JObject)input);
                }

                foreach (var e in _input)
                    if (((JObject)e).ContainsKey("@context"))
                        dataset.ParseContext((JObject)e["@context"]);
            }

            if (callback != null) return callback.Call(dataset);
            if (options.format != null)
                if ("application/nquads".Equals(options.format))
                {
                    return new NQuadTripleCallback().Call(dataset);
                }
                else
                {
                    if ("text/turtle".Equals(options.format))
                        return new TurtleTripleCallback().Call(dataset);
                    throw new JsonLdError(JsonLdError.Error.UnknownFormat, options.format);
                }

            return dataset;
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<object> ToRdfAsync(JToken input, JsonLdOptions options)
        {
            return ToRdfAsync(input, null, options);
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<object> ToRdfAsync(JToken input, IJsonLdTripleCallback callback)
        {
            return ToRdfAsync(input, callback, new JsonLdOptions(string.Empty));
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<object> ToRdfAsync(JToken input)
        {
            return ToRdfAsync(input, new JsonLdOptions(string.Empty));
        }

        /// <summary>Performs RDF dataset normalization on the given JSON-LD input.</summary>
        /// <remarks>
        ///     Performs RDF dataset normalization on the given JSON-LD input. The output
        ///     is an RDF dataset unless the 'format' option is used.
        /// </remarks>
        /// <param name="input">the JSON-LD input to normalize.</param>
        /// <param name="options"></param>
        /// <exception cref="JsonLdError"></exception>
        public static async Task<object> NormalizeAsync(JToken input, JsonLdOptions options)
        {
            var opts = options.Clone();
            opts.format = null;
            var dataset = (RdfDataset)await ToRdfAsync(input, opts);
            return new JsonLdApi(options).Normalize(dataset);
        }

        /// <exception cref="JsonLdError"></exception>
        public static Task<object> NormalizeAsync(JToken input)
        {
            return NormalizeAsync(input, new JsonLdOptions(string.Empty));
        }

        private sealed class RdfParserDictionary : Dictionary<string, IRdfParser>
        {
            public RdfParserDictionary()
            {
                {
                    // automatically register nquad serializer
                    this["application/nquads"] = new NQuadRdfParser();
                    this["text/turtle"] = new TurtleRdfParser();
                }
            }
        }
    }
}