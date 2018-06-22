using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    public class RdfDatasetUtils
    {
        private static readonly Pattern UcharMatched = Pattern.Compile("\\u005C(?:([tbnrf\\\"'])|(?:u("
                                                                       + Core.Regex.Hex + "{4}))|(?:U(" + Core.Regex.Hex + "{8})))"
        );

        /// <summary>Creates an array of RDF triples for the given graph.</summary>
        /// <remarks>Creates an array of RDF triples for the given graph.</remarks>
        /// <param name="graph">the graph to create RDF triples for.</param>
        /// <param name="namer">a UniqueNamer for assigning blank node names.</param>
        /// <returns>the array of RDF triples for the given graph.</returns>
        [Obsolete]
        internal static JArray GraphToRDF(JObject graph,
                                          UniqueNamer
                                              namer)
        {
            // use RdfDataset.graphToRDF
            var rval = new JArray();
            foreach (var id in graph.GetKeys())
            {
                var node = (JObject)graph[id];
                var properties = new JArray(node.GetKeys());
                properties.SortInPlace();
                foreach (string property in properties)
                {
                    var eachProperty = property;
                    var items = node[eachProperty];
                    if ("@type".Equals(eachProperty))
                    {
                        eachProperty = JsonLdConsts.RdfType;
                    }
                    else
                    {
                        if (JsonLdUtils.IsKeyword(eachProperty)) continue;
                    }

                    foreach (var item in (JArray)items)
                    {
                        // RDF subjects
                        var subject = new JObject();
                        if (id.IndexOf("_:") == 0)
                        {
                            subject["type"] = "blank node";
                            subject["value"] = namer.GetName(id);
                        }
                        else
                        {
                            subject["type"] = "IRI";
                            subject["value"] = id;
                        }

                        // RDF predicates
                        var predicate = new JObject();
                        predicate["type"] = "IRI";
                        predicate["value"] = eachProperty;

                        // convert @list to triples
                        if (JsonLdUtils.IsList(item))
                        {
                            ListToRDF((JArray)((JObject)item)["@list"], namer, subject, predicate, rval);
                        }
                        else
                        {
                            // convert value or node object to triple
                            object @object = ObjectToRDF(item, namer);
                            IDictionary<string, object> tmp = new Dictionary<string, object>();
                            tmp["subject"] = subject;
                            tmp["predicate"] = predicate;
                            tmp["object"] = @object;
                            rval.Add(tmp);
                        }
                    }
                }
            }

            return rval;
        }

        /// <summary>
        ///     Converts a @list value into linked list of blank node RDF triples (an RDF
        ///     collection).
        /// </summary>
        /// <remarks>
        ///     Converts a @list value into linked list of blank node RDF triples (an RDF
        ///     collection).
        /// </remarks>
        /// <param name="list">the @list value.</param>
        /// <param name="namer">a UniqueNamer for assigning blank node names.</param>
        /// <param name="subject">the subject for the head of the list.</param>
        /// <param name="predicate">the predicate for the head of the list.</param>
        /// <param name="triples">the array of triples to append to.</param>
        private static void ListToRDF(JArray list,
                                      UniqueNamer namer,
                                      JObject subject,
                                      JObject predicate,
                                      JArray triples
        )
        {
            var first = new JObject();
            first["type"] = "IRI";
            first["value"] = JsonLdConsts.RdfFirst;
            var rest = new JObject();
            rest["type"] = "IRI";
            rest["value"] = JsonLdConsts.RdfRest;
            var nil = new JObject();
            nil["type"] = "IRI";
            nil["value"] = JsonLdConsts.RdfNil;
            foreach (var item in list)
            {
                var blankNode = new JObject();
                blankNode["type"] = "blank node";
                blankNode["value"] = namer.GetName();
                {
                    var tmp = new JObject();
                    tmp["subject"] = subject;
                    tmp["predicate"] = predicate;
                    tmp["object"] = blankNode;
                    triples.Add(tmp);
                }
                subject = blankNode;
                predicate = first;
                JToken @object = ObjectToRDF(item, namer);
                {
                    var tmp = new JObject();
                    tmp["subject"] = subject;
                    tmp["predicate"] = predicate;
                    tmp["object"] = @object;
                    triples.Add(tmp);
                }
                predicate = rest;
            }

            var tmp_1 = new JObject();
            tmp_1["subject"] = subject;
            tmp_1["predicate"] = predicate;
            tmp_1["object"] = nil;
            triples.Add(tmp_1);
        }

        /// <summary>
        ///     Converts a JSON-LD value object to an RDF literal or a JSON-LD string or
        ///     node object to an RDF resource.
        /// </summary>
        /// <remarks>
        ///     Converts a JSON-LD value object to an RDF literal or a JSON-LD string or
        ///     node object to an RDF resource.
        /// </remarks>
        /// <param name="item">the JSON-LD value or node object.</param>
        /// <param name="namer">the UniqueNamer to use to assign blank node names.</param>
        /// <returns>the RDF literal or RDF resource.</returns>
        private static JObject ObjectToRDF(JToken item, UniqueNamer namer)
        {
            var @object = new JObject();

            // convert value object to RDF
            if (JsonLdUtils.IsValue(item))
            {
                @object["type"] = "literal";
                var value = ((JObject)item)["@value"];
                var datatype = ((JObject)item)["@type"];

                // convert to XSD datatypes as appropriate
                if (value.Type == JTokenType.Boolean || value.Type == JTokenType.Float || value.Type == JTokenType.Integer)
                {
                    // convert to XSD datatype
                    if (value.Type == JTokenType.Boolean)
                    {
                        @object["value"] = value.ToString();
                        @object["datatype"] = datatype.IsNull() ? JsonLdConsts.XsdBoolean : datatype;
                    }
                    else
                    {
                        if (value.Type == JTokenType.Float)
                        {
                            // canonical double representation
                            @object["value"] = string.Format("{0:0.0###############E0}", (double)value);
                            @object["datatype"] = datatype.IsNull() ? JsonLdConsts.XsdDouble : datatype;
                        }
                        else
                        {
                            var df = new DecimalFormat("0");
                            @object["value"] = df.Format((int)value);
                            @object["datatype"] = datatype.IsNull() ? JsonLdConsts.XsdInteger : datatype;
                        }
                    }
                }
                else
                {
                    if (((IDictionary<string, JToken>)item).ContainsKey("@language"))
                    {
                        @object["value"] = value;
                        @object["datatype"] = datatype.IsNull() ? JsonLdConsts.RdfLangstring : datatype;
                        @object["language"] = ((IDictionary<string, JToken>)item)["@language"];
                    }
                    else
                    {
                        @object["value"] = value;
                        @object["datatype"] = datatype.IsNull() ? JsonLdConsts.XsdString : datatype;
                    }
                }
            }
            else
            {
                // convert string/node object to RDF
                var id = JsonLdUtils.IsObject(item)
                             ? (string)((JObject)item
                                       )["@id"]
                             : (string)item;
                if (id.IndexOf("_:") == 0)
                {
                    @object["type"] = "blank node";
                    @object["value"] = namer.GetName(id);
                }
                else
                {
                    @object["type"] = "IRI";
                    @object["value"] = id;
                }
            }

            return @object;
        }

        public static string ToNQuads(RdfDataset dataset)
        {
            IList<string> quads = new List<string>();
            foreach (var graphName in dataset.GraphNames())
            {
                var eachGraphName = graphName;
                var triples = dataset.GetQuads(eachGraphName);
                if ("@default".Equals(eachGraphName)) eachGraphName = null;
                foreach (var triple in triples) quads.Add(ToNQuad(triple, eachGraphName));
            }

            ((List<string>)quads).Sort(StringComparer.Ordinal);

            var rval = string.Empty;
            foreach (var quad in quads) rval += quad;
            return rval;
        }

        internal static string ToNQuad(RdfDataset.Quad triple,
                                       string graphName,
                                       string bnode
        )
        {
            var s = triple.GetSubject();
            var p = triple.GetPredicate();
            var o = triple.GetObject();
            var quad = string.Empty;

            // subject is an IRI or bnode
            if (s.IsIRI())
            {
                quad += "<" + Escape(s.GetValue()) + ">";
            }
            else
            {
                // normalization mode
                if (bnode != null)
                    quad += bnode.Equals(s.GetValue()) ? "_:a" : "_:z";
                else
                    quad += s.GetValue();
            }

            if (p.IsIRI())
                quad += " <" + Escape(p.GetValue()) + "> ";
            else
                quad += " " + Escape(p.GetValue()) + " ";

            // object is IRI, bnode or literal
            if (o.IsIRI())
            {
                quad += "<" + Escape(o.GetValue()) + ">";
            }
            else
            {
                if (o.IsBlankNode())
                {
                    // normalization mode
                    if (bnode != null)
                        quad += bnode.Equals(o.GetValue()) ? "_:a" : "_:z";
                    else
                        quad += o.GetValue();
                }
                else
                {
                    var escaped = Escape(o.GetValue());
                    quad += "\"" + escaped + "\"";
                    if (JsonLdConsts.RdfLangstring.Equals(o.GetDatatype()))
                    {
                        quad += "@" + o.GetLanguage();
                    }
                    else
                    {
                        if (!JsonLdConsts.XsdString.Equals(o.GetDatatype())) quad += "^^<" + Escape(o.GetDatatype()) + ">";
                    }
                }
            }

            // graph
            if (graphName != null)
                if (graphName.IndexOf("_:") != 0)
                {
                    quad += " <" + Escape(graphName) + ">";
                }
                else
                {
                    if (bnode != null)
                        quad += " _:g";
                    else
                        quad += " " + graphName;
                }

            quad += " .\n";
            return quad;
        }

        internal static string ToNQuad(RdfDataset.Quad triple, string graphName)
        {
            return ToNQuad(triple, graphName, null);
        }

        public static string Unescape(string str)
        {
            var rval = str;
            if (str != null)
            {
                var m = UcharMatched.Matcher(str);
                while (m.Find())
                {
                    var uni = m.Group(0);
                    if (m.Group(1) == null)
                    {
                        var hex = m.Group(2) != null ? m.Group(2) : m.Group(3);
                        var v = Convert.ToInt32(hex, 16);

                        // hex =
                        // hex.replaceAll("^(?:00)+",
                        // "");
                        if (v > 0xFFFF)
                        {
                            // deal with UTF-32
                            // Integer v = Integer.parseInt(hex, 16);
                            var vt = v - 0x10000;
                            var vh = vt >> 10;
                            var v1 = vt & 0x3FF;
                            var w1 = 0xD800 + vh;
                            var w2 = 0xDC00 + v1;
                            var b = new StringBuilder();
                            b.AppendCodePoint(w1);
                            b.AppendCodePoint(w2);
                            uni = b.ToString();
                        }
                        else
                        {
                            uni = char.ToString((char)v);
                        }
                    }
                    else
                    {
                        var c = m.Group(1)[0];
                        switch (c)
                        {
                            case 'b':
                            {
                                uni = "\b";
                                break;
                            }

                            case 'n':
                            {
                                uni = "\n";
                                break;
                            }

                            case 't':
                            {
                                uni = "\t";
                                break;
                            }

                            case 'f':
                            {
                                uni = "\f";
                                break;
                            }

                            case 'r':
                            {
                                uni = "\r";
                                break;
                            }

                            case '\'':
                            {
                                uni = "'";
                                break;
                            }

                            case '\"':
                            {
                                uni = "\"";
                                break;
                            }

                            case '\\':
                            {
                                uni = "\\";
                                break;
                            }

                            default:
                            {
                                // do nothing
                                continue;
                            }
                        }
                    }

                    var pat = Pattern.Quote(m.Group(0));
                    var x = JavaCompat.ToHexString(uni[0]);
                    rval = rval.Replace(pat, uni);
                }
            }

            return rval;
        }

        public static string Escape(string str)
        {
            var rval = string.Empty;
            for (var i = 0; i < str.Length; i++)
            {
                var hi = str[i];
                if (hi <= 0x8 || hi == 0xB || hi == 0xC || hi >= 0xE && hi <= 0x1F ||
                    hi >= 0x7F && hi <= 0xA0 || hi >= 0x24F && !char.IsHighSurrogate(hi))
                {
                    // 0xA0 is end of
                    // non-printable latin-1
                    // supplement
                    // characters
                    // 0x24F is the end of latin extensions
                    // TODO: there's probably a lot of other characters that
                    // shouldn't be escaped that
                    // fall outside these ranges, this is one example from the
                    // json-ld tests
                    rval += string.Format("\\u%04x", (int)hi);
                }
                else
                {
                    if (char.IsHighSurrogate(hi))
                    {
                        var lo = str[++i];
                        var c = (hi << 10) + lo + (0x10000 - (0xD800
                                                              << 10) - 0xDC00);
                        rval += string.Format("\\U%08x", c);
                    }
                    else
                    {
                        switch (hi)
                        {
                            case '\b':
                            {
                                rval += "\\b";
                                break;
                            }

                            case '\n':
                            {
                                rval += "\\n";
                                break;
                            }

                            case '\t':
                            {
                                rval += "\\t";
                                break;
                            }

                            case '\f':
                            {
                                rval += "\\f";
                                break;
                            }

                            case '\r':
                            {
                                rval += "\\r";
                                break;
                            }

                            case '\"':
                            {
                                // case '\'':
                                // rval += "\\'";
                                // break;
                                rval += "\\\"";

                                // rval += "\\u0022";
                                break;
                            }

                            case '\\':
                            {
                                rval += "\\\\";
                                break;
                            }

                            default:
                            {
                                // just put the char as is
                                rval += hi;
                                break;
                            }
                        }
                    }
                }
            }

            return rval;
        }

        /// <summary>Parses RDF in the form of N-Quads.</summary>
        /// <remarks>Parses RDF in the form of N-Quads.</remarks>
        /// <param name="input">the N-Quads input to parse.</param>
        /// <returns>an RDF dataset.</returns>
        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        public static RdfDataset ParseNQuads(string input)
        {
            // build RDF dataset
            var dataset = new RdfDataset();

            // split N-Quad input into lines
            var lines = Regex.Eoln.Split(input);
            var lineNumber = 0;
            foreach (var line in lines)
            {
                lineNumber++;

                // skip empty lines
                if (Regex.EmptyOrComment.Matcher(line).Matches()) continue;

                // parse quad
                var match = Regex.Quad.Matcher(line);
                if (!match.Matches())
                    throw new JsonLdError(JsonLdError.Error.SyntaxError,
                                          "Error while parsing N-Quads; invalid quad. line:"
                                          + lineNumber);

                // get subject
                RdfDataset.Node subject;
                if (match.Group(1) != null)
                {
                    var subjectIri = Unescape(match.Group(1));
                    AssertAbsoluteIri(subjectIri);
                    subject = new RdfDataset.IRI(subjectIri);
                }
                else
                {
                    subject = new RdfDataset.BlankNode(Unescape(match.Group(2)));
                }

                // get predicate
                var predicateIri = Unescape(match.Group(3));
                AssertAbsoluteIri(predicateIri);
                RdfDataset.Node predicate = new RdfDataset.IRI(predicateIri);

                // get object
                RdfDataset.Node @object;
                if (match.Group(4) != null)
                {
                    var objectIri = Unescape(match.Group(4));
                    AssertAbsoluteIri(objectIri);
                    @object = new RdfDataset.IRI(objectIri);
                }
                else
                {
                    if (match.Group(5) != null)
                    {
                        @object = new RdfDataset.BlankNode(Unescape(match.Group(5)));
                    }
                    else
                    {
                        var language = Unescape(match.Group(8));
                        var datatype = match.Group(7) != null ? Unescape(match.Group(7)) :
                                       match.Group
                                           (8) != null ? JsonLdConsts.RdfLangstring : JsonLdConsts.XsdString;
                        AssertAbsoluteIri(datatype);
                        var unescaped = Unescape(match.Group(6));
                        @object = new RdfDataset.Literal(unescaped, datatype, language);
                    }
                }

                // get graph name ('@default' is used for the default graph)
                var name = "@default";
                if (match.Group(9) != null)
                {
                    name = Unescape(match.Group(9));
                    AssertAbsoluteIri(name);
                }
                else
                {
                    if (match.Group(10) != null) name = Unescape(match.Group(10));
                }

                var triple = new RdfDataset.Quad(subject, predicate, @object, name);

                // initialise graph in dataset
                if (!dataset.ContainsKey(name))
                {
                    IList<RdfDataset.Quad> tmp = new List<RdfDataset.Quad>();
                    tmp.Add(triple);
                    dataset[name] = tmp;
                }
                else
                {
                    // add triple if unique to its graph
                    var triples = (IList<RdfDataset.Quad>)dataset[name];
                    if (!triples.Contains(triple)) triples.Add(triple);
                }
            }

            return dataset;
        }

        private static void AssertAbsoluteIri(string iri)
        {
            if (Uri.IsWellFormedUriString(Uri.EscapeUriString(iri), UriKind.Absolute) == false)
                throw new JsonLdError(JsonLdError.Error.SyntaxError, "Invalid absolute URI <" + iri + ">");
        }

        private class Regex
        {
            public static readonly Pattern HEX = Pattern.Compile("[0-9A-Fa-f]");

            public static readonly Pattern UCHAR = Pattern.Compile("\\\\u" + HEX + "{4}|\\\\U" + HEX + "{8}");

            public static readonly Pattern Iri = Pattern.Compile("(?:<((?:[^\\x00-\\x20<>\"{}|^`\\\\]|" + UCHAR + ")*)>)");

            public static readonly Pattern Bnode = Pattern.Compile("(_:(?:[A-Za-z0-9](?:[A-Za-z0-9\\-\\.]*[A-Za-z0-9])?))"
            );

            public static readonly Pattern ECHAR = Pattern.Compile("\\\\[tbnrf\"'\\\\]");

            public static readonly Pattern Plain = Pattern.Compile("\"((?:[^\\x22\\x5C\\x0A\\x0D]|" + ECHAR + "|" + UCHAR + ")*)\"");

            public static readonly Pattern Datatype = Pattern.Compile("(?:\\^\\^" + Iri + ")"
            );

            public static readonly Pattern Language = Pattern.Compile("(?:@([a-z]+(?:-[a-zA-Z0-9]+)*))"
            );

            public static readonly Pattern Literal = Pattern.Compile("(?:" + Plain + "(?:" +
                                                                     Datatype + "|" + Language + ")?)");

            public static readonly Pattern Wso = Pattern.Compile("[ \\t]*");

            public static readonly Pattern Eoln = Pattern.Compile("(?:\r\n)|(?:\n)|(?:\r)");

            public static readonly Pattern EmptyOrComment = Pattern.Compile("^" + Wso + "(#.*)?$");

            public static readonly Pattern Subject = Pattern.Compile("(?:" + Iri + "|" + Bnode
                                                                     + ")" + Wso);

            public static readonly Pattern Property = Pattern.Compile(Iri.GetPattern() + Wso);

            public static readonly Pattern Object = Pattern.Compile("(?:" + Iri + "|" + Bnode
                                                                    + "|" + Literal + ")" + Wso);

            public static readonly Pattern Graph = Pattern.Compile("(?:\\.|(?:(?:" + Iri + "|"
                                                                   + Bnode + ")" + Wso + "\\.))");

            public static readonly Pattern Quad = Pattern.Compile("^" + Wso + Subject + Property
                                                                  + Object + Graph + Wso + "(#.*)?$");

            // define partial regexes
            // final public static Pattern IRI =
            // Pattern.compile("(?:<([^:]+:[^>]*)>)");
            // define quad part regexes
            // full quad regex
        }
    }
}