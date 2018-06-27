using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    /// <summary>
    ///     Starting to migrate away from using plain java Maps as the internal RDF
    ///     dataset store.
    /// </summary>
    /// <remarks>
    ///     Starting to migrate away from using plain java Maps as the internal RDF
    ///     dataset store. Currently each item just wraps a Map based on the old format
    ///     so everything doesn't break. Will phase this out once everything is using the
    ///     new format.
    /// </remarks>
    /// <author>Tristan</author>
    //[System.Serializable]
    public class RdfDataset : Dictionary<string, object>
    {
        private static readonly Node first = new IRI(JsonLdConsts.RdfFirst
        );

        private static readonly Node rest = new IRI(JsonLdConsts.RdfRest
        );

        private static readonly Node nil = new IRI(JsonLdConsts.RdfNil
        );

        private readonly IDictionary<string, string> context;

        private readonly JsonLdApi api;

        public RdfDataset()
        {
            // private UniqueNamer namer;
            this["@default"] = new List<Quad>();
            context = new Dictionary<string, string>();
        }

        public RdfDataset(JsonLdApi jsonLdApi) : this()
        {
            // put("@context", context);
            api = jsonLdApi;
        }

        public virtual void SetNamespace(string ns, string prefix)
        {
            context[ns] = prefix;
        }

        public virtual string GetNamespace(string ns)
        {
            return context[ns];
        }

        /// <summary>clears all the namespaces in this dataset</summary>
        public virtual void ClearNamespaces()
        {
            context.Clear();
        }

        public virtual IDictionary<string, string> GetNamespaces()
        {
            return context;
        }

        /// <summary>Returns a valid @context containing any namespaces set</summary>
        /// <returns></returns>
        public virtual JObject GetContext()
        {
            var rval = new JObject();
            rval.PutAll(context);

            // replace "" with "@vocab"
            if (rval.ContainsKey(string.Empty)) rval["@vocab"] = Collections.Remove(rval, string.Empty);
            return rval;
        }

        /// <summary>parses a @context object and sets any namespaces found within it</summary>
        /// <param name="context"></param>
        public virtual void ParseContext(JObject context)
        {
            foreach (var key in context.GetKeys())
            {
                var val = context[key];
                if ("@vocab".Equals(key))
                {
                    if (val.IsNull() || JsonLdUtils.IsString(val)) SetNamespace(string.Empty, (string)val);
                }
                else
                {
                    // TODO: the context is actually invalid, should we throw an
                    // exception?
                    if ("@context".Equals(key))
                    {
                        // go deeper!
                        ParseContext((JObject)context["@context"]);
                    }
                    else
                    {
                        if (!JsonLdUtils.IsKeyword(key))
                            if (val.Type == JTokenType.String)
                            {
                                SetNamespace(key, (string)context[key]);
                            }
                            else
                            {
                                if (JsonLdUtils.IsObject(val) && ((JObject)val).ContainsKey("@id"
                                    ))
                                    SetNamespace(key, (string)((JObject)val)["@id"]);
                            }
                    }
                }
            }
        }

        /// <summary>Adds a triple to the @default graph of this dataset</summary>
        /// <param name="s">the subject for the triple</param>
        /// <param name="p">the predicate for the triple</param>
        /// <param name="value">the value of the literal object for the triple</param>
        /// <param name="datatype">
        ///     the datatype of the literal object for the triple (null values
        ///     will default to xsd:string)
        /// </param>
        /// <param name="language">
        ///     the language of the literal object for the triple (or null)
        /// </param>
        public virtual void AddTriple(string s,
                                      string p,
                                      string value,
                                      string datatype,
                                      string language)
        {
            AddQuad(s, p, value, datatype, language, "@default");
        }

        /// <summary>Adds a triple to the specified graph of this dataset</summary>
        /// <param name="s">the subject for the triple</param>
        /// <param name="p">the predicate for the triple</param>
        /// <param name="value">the value of the literal object for the triple</param>
        /// <param name="datatype">
        ///     the datatype of the literal object for the triple (null values
        ///     will default to xsd:string)
        /// </param>
        /// <param name="graph">the graph to add this triple to</param>
        /// <param name="language">
        ///     the language of the literal object for the triple (or null)
        /// </param>
        public virtual void AddQuad(string s,
                                    string p,
                                    string value,
                                    string datatype,
                                    string
                                        language,
                                    string graph)
        {
            if (graph == null) graph = "@default";
            if (!ContainsKey(graph)) this[graph] = new List<Quad>();
            ((IList<Quad>)this[graph]).Add(new Quad(s, p, value, datatype, language, graph));
        }

        /// <summary>Adds a triple to the @default graph of this dataset</summary>
        /// <param name="s">the subject for the triple</param>
        /// <param name="p">the predicate for the triple</param>
        /// <param name="o">the object for the triple</param>
        /// <param name="datatype">
        ///     the datatype of the literal object for the triple (null values
        ///     will default to xsd:string)
        /// </param>
        /// <param name="language">
        ///     the language of the literal object for the triple (or null)
        /// </param>
        public virtual void AddTriple(string s, string p, string o)
        {
            AddQuad(s, p, o, "@default");
        }

        /// <summary>Adds a triple to thespecified graph of this dataset</summary>
        /// <param name="s">the subject for the triple</param>
        /// <param name="p">the predicate for the triple</param>
        /// <param name="o">the object for the triple</param>
        /// <param name="datatype">
        ///     the datatype of the literal object for the triple (null values
        ///     will default to xsd:string)
        /// </param>
        /// <param name="graph">the graph to add this triple to</param>
        /// <param name="language">
        ///     the language of the literal object for the triple (or null)
        /// </param>
        public virtual void AddQuad(string s, string p, string o, string graph)
        {
            if (graph == null) graph = "@default";
            if (!ContainsKey(graph)) this[graph] = new List<Quad>();
            ((IList<Quad>)this[graph]).Add(new Quad(s, p, o, graph));
        }

        /// <summary>Creates an array of RDF triples for the given graph.</summary>
        /// <remarks>Creates an array of RDF triples for the given graph.</remarks>
        /// <param name="graph">the graph to create RDF triples for.</param>
        internal virtual void GraphToRDF(string graphName,
                                         JObject graph
        )
        {
            // 4.2)
            IList<Quad> triples = new List<Quad>();

            // 4.3)
            var subjects = graph.GetKeys();

            // Collections.sort(subjects);
            foreach (var id in subjects)
            {
                if (JsonLdUtils.IsRelativeIri(id)) continue;
                var node = (JObject)graph[id];
                var properties = new JArray(node.GetKeys());
                properties.SortInPlace();
                foreach (string property in properties)
                {
                    var localProperty = property;
                    JArray values;

                    // 4.3.2.1)
                    if ("@type".Equals(localProperty))
                    {
                        values = (JArray)node["@type"];
                        localProperty = JsonLdConsts.RdfType;
                    }
                    else
                    {
                        // 4.3.2.2)
                        if (JsonLdUtils.IsKeyword(localProperty)) continue;

                        // 4.3.2.3)
                        if (localProperty.StartsWith("_:") && !api.opts.GetProduceGeneralizedRdf()) continue;

                        // 4.3.2.4)
                        if (JsonLdUtils.IsRelativeIri(localProperty))
                            continue;
                        values = (JArray)node[localProperty];
                    }

                    Node subject;
                    if (id.IndexOf("_:") == 0)
                        subject = new BlankNode(id);
                    else
                        subject = new IRI(id);

                    // RDF predicates
                    Node predicate;
                    if (localProperty.StartsWith("_:"))
                        predicate = new BlankNode(localProperty);
                    else
                        predicate = new IRI(localProperty);
                    foreach (var item in values)

                        // convert @list to triples
                        if (JsonLdUtils.IsList(item))
                        {
                            var list = (JArray)((JObject)item)["@list"];
                            Node last = null;
                            var firstBNode = nil;
                            if (!list.IsEmpty())
                            {
                                last = ObjectToRDF(list[list.Count - 1]);
                                firstBNode = new BlankNode(api.GenerateBlankNodeIdentifier());
                            }

                            triples.Add(new Quad(subject, predicate, firstBNode, graphName));
                            for (var i = 0; i < list.Count - 1; i++)
                            {
                                var @object = ObjectToRDF(list[i]);
                                triples.Add(new Quad(firstBNode, first, @object, graphName));
                                Node restBNode = new BlankNode(api.GenerateBlankNodeIdentifier
                                                                   ());
                                triples.Add(new Quad(firstBNode, rest, restBNode, graphName));
                                firstBNode = restBNode;
                            }

                            if (last != null)
                            {
                                triples.Add(new Quad(firstBNode, first, last, graphName));
                                triples.Add(new Quad(firstBNode, rest, nil, graphName));
                            }
                        }
                        else
                        {
                            // convert value or node object to triple
                            var @object = ObjectToRDF(item);
                            if (@object != null) triples.Add(new Quad(subject, predicate, @object, graphName));
                        }
                }
            }

            this[graphName] = triples;
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
        private Node ObjectToRDF(JToken item)
        {
            // convert value object to RDF
            if (JsonLdUtils.IsValue(item))
            {
                var value = ((JObject)item)["@value"];
                var datatype = ((JObject)item)["@type"];

                // convert to XSD datatypes as appropriate
                if (value.Type == JTokenType.Boolean || value.Type == JTokenType.Float || value.Type == JTokenType.Integer)
                    if (value.Type == JTokenType.Boolean)
                    {
                        var serializeObject = JsonConvert.SerializeObject(value, Formatting.None).Trim('"');
                        return new Literal(serializeObject,
                                           datatype.IsNull()
                                               ? JsonLdConsts.XsdBoolean
                                               : (string)datatype,
                                           null);
                    }
                    else
                    {
                        if (value.Type == JTokenType.Float || datatype.SafeCompare(JsonLdConsts.XsdDouble))
                        {
                            // Workaround for Newtonsoft.Json's refusal to cast a JTokenType.Integer to a double.
                            if (value.Type == JTokenType.Integer)
                            {
                                var number = (int)value;
                                value = new JValue((double)number);
                            }

                            // canonical double representation
                            return new Literal(string.Format(CultureInfo.InvariantCulture, "{0:0.0###############E0}", (double)value),
                                               datatype.IsNull()
                                                   ? JsonLdConsts.XsdDouble
                                                   : (string)datatype,
                                               null);
                        }

                        return new Literal(string.Format("{0:0}", value),
                                           datatype.IsNull()
                                               ? JsonLdConsts.XsdInteger
                                               : (string)datatype,
                                           null);
                    }

                if (((JObject)item).ContainsKey("@language"))
                    return new Literal((string)value,
                                       datatype.IsNull()
                                           ? JsonLdConsts.RdfLangstring
                                           : (string)datatype,
                                       (string)((JObject)item)["@language"]);

                {
                    var serializeObject = JsonConvert.SerializeObject(value, Formatting.None).Trim('"');
                    return new Literal(serializeObject,
                                       datatype.IsNull()
                                           ? JsonLdConsts.XsdString
                                           : (string)datatype,
                                       null);
                }
            }

            // convert string/node object to RDF
            string id;
            if (JsonLdUtils.IsObject(item))
            {
                id = (string)((JObject)item)["@id"];
                if (JsonLdUtils.IsRelativeIri(id)) return null;
            }
            else
            {
                id = (string)item;
            }

            if (id.IndexOf("_:") == 0)
                return new BlankNode(id);
            return new IRI(id);
        }

        public virtual IList<string> GraphNames()
        {
            return new List<string>(Keys);
        }

        public virtual IList<Quad> GetQuads(string graphName)
        {
            return (IList<Quad>)this[graphName];
        }

        //[System.Serializable]
        public class Quad : Dictionary<string, object>, IComparable<Quad>
        {
            public Quad(string subject, string predicate, string @object, string graph) : this(subject,
                                                                                               predicate,
                                                                                               @object.StartsWith("_:")
                                                                                                   ? new BlankNode(@object
                                                                                                   )
                                                                                                   : (Node)new IRI(@object),
                                                                                               graph)
            {
            }

            public Quad(string subject,
                        string predicate,
                        string value,
                        string datatype,
                        string
                            language,
                        string graph) : this(subject, predicate, new Literal(value, datatype, language), graph)
            {
            }

            private Quad(string subject,
                         string predicate,
                         Node @object,
                         string graph
            ) : this(subject.StartsWith("_:")
                         ? new BlankNode(subject)
                         : (Node)new IRI
                             (subject),
                     new IRI(predicate),
                     @object,
                     graph)
            {
            }

            public Quad(Node subject, Node predicate, Node @object, string graph)
            {
                this["subject"] = subject;
                this["predicate"] = predicate;
                this["object"] = @object;
                if (graph != null && !"@default".Equals(graph))
                    this["name"] = graph.StartsWith("_:")
                                       ? new BlankNode(graph)
                                       : (Node)new IRI
                                           (graph);
            }

            public virtual int CompareTo(Quad o)
            {
                if (o == null) return 1;
                var rval = GetGraph().CompareTo(o.GetGraph());
                if (rval != 0) return rval;
                rval = GetSubject().CompareTo(o.GetSubject());
                if (rval != 0) return rval;
                rval = GetPredicate().CompareTo(o.GetPredicate());
                if (rval != 0) return rval;
                return GetObject().CompareTo(o.GetObject());
            }

            public virtual Node GetSubject()
            {
                return (Node)this["subject"];
            }

            public virtual Node GetPredicate()
            {
                return (Node)this["predicate"];
            }

            public virtual Node GetObject()
            {
                return (Node)this["object"];
            }

            public virtual Node GetGraph()
            {
                return (Node)this["name"];
            }
        }

        //[System.Serializable]
        public abstract class Node : Dictionary<string, object>, IComparable<Node
                                     >
        {
            public virtual int CompareTo(Node o)
            {
                if (IsIRI())
                {
                    if (!o.IsIRI()) return 1;
                }
                else
                {
                    if (IsBlankNode())
                        if (o.IsIRI())
                        {
                            // IRI > blank node
                            return -1;
                        }
                        else
                        {
                            if (o.IsLiteral()) return 1;
                        }
                }

                return string.CompareOrdinal(GetValue(), o.GetValue());
            }

            public abstract bool IsLiteral();

            public abstract bool IsIRI();

            public abstract bool IsBlankNode();

            public virtual string GetValue()
            {
                object value;
                return TryGetValue("value", out value) ? (string)value : null;
            }

            public virtual string GetDatatype()
            {
                object value;
                return TryGetValue("datatype", out value) ? (string)value : null;
            }

            public virtual string GetLanguage()
            {
                object value;
                return TryGetValue("language", out value) ? (string)value : null;
            }

            /// <summary>Converts an RDF triple object to a JSON-LD object.</summary>
            /// <remarks>Converts an RDF triple object to a JSON-LD object.</remarks>
            /// <param name="o">the RDF triple object to convert.</param>
            /// <param name="useNativeTypes">true to output native types, false not to.</param>
            /// <returns>the JSON-LD object.</returns>
            /// <exception cref="JsonLdError">JsonLdError</exception>
            /// <exception cref="JsonLD.Core.JsonLdError"></exception>
            internal virtual JObject ToObject(bool useNativeTypes)
            {
                // If value is an an IRI or a blank node identifier, return a new
                // JSON object consisting
                // of a single member @id whose value is set to value.
                if (IsIRI() || IsBlankNode())
                {
                    var obj = new JObject();
                    obj["@id"] = GetValue();
                    return obj;
                }

                // convert literal object to JSON-LD
                var rval = new JObject();
                rval["@value"] = GetValue();

                // add language
                if (GetLanguage() != null)
                {
                    rval["@language"] = GetLanguage();
                }
                else
                {
                    // add datatype
                    var type = GetDatatype();
                    var value = GetValue();
                    if (useNativeTypes)
                    {
                        // use native datatypes for certain xsd types
                        if (JsonLdConsts.XsdString.Equals(type))
                        {
                        }
                        else
                        {
                            // don't add xsd:string
                            if (JsonLdConsts.XsdBoolean.Equals(type))
                            {
                                if ("true".Equals(value))
                                {
                                    rval["@value"] = true;
                                }
                                else
                                {
                                    if ("false".Equals(value)) rval["@value"] = false;
                                }
                            }
                            else
                            {
                                if (Pattern.Matches(value, "^[+-]?[0-9]+((?:\\.?[0-9]+((?:E?[+-]?[0-9]+)|)|))$"))
                                    try
                                    {
                                        var d = double.Parse(value, CultureInfo.InvariantCulture);
                                        if (!double.IsNaN(d) && !double.IsInfinity(d))
                                            if (JsonLdConsts.XsdInteger.Equals(type))
                                            {
                                                var i = (int)d;
                                                if (i.ToString().Equals(value)) rval["@value"] = i;
                                            }
                                            else
                                            {
                                                if (JsonLdConsts.XsdDouble.Equals(type))
                                                    rval["@value"] = d;
                                                else
                                                    rval["@type"] = type;
                                            }
                                    }
                                    catch
                                    {
                                        // TODO: This should never happen since we match the
                                        // value with regex!
                                        throw;
                                    }
                                else
                                    rval["@type"] = type;
                            }
                        }
                    }
                    else
                    {
                        if (!JsonLdConsts.XsdString.Equals(type)) rval["@type"] = type;
                    }
                }

                return rval;
            }
        }

        //[System.Serializable]
        public class Literal : Node
        {
            public Literal(string value, string datatype, string language)
            {
                this["type"] = "literal";
                this["value"] = value;
                this["datatype"] = datatype != null ? datatype : JsonLdConsts.XsdString;
                if (language != null) this["language"] = language;
            }

            public override bool IsLiteral()
            {
                return true;
            }

            public override bool IsIRI()
            {
                return false;
            }

            public override bool IsBlankNode()
            {
                return false;
            }

            public override int CompareTo(Node o)
            {
                if (o == null) return 1;
                if (o.IsIRI()) return -1;
                if (o.IsBlankNode()) return -1;
                if (GetLanguage() == null && ((Literal)o).GetLanguage() != null) return -1;

                if (GetLanguage() != null && ((Literal)o).GetLanguage() == null) return 1;
                if (GetDatatype() != null)
                {
                    return string.CompareOrdinal(GetDatatype(),
                                                 ((Literal)o).GetDatatype
                                                     ());
                }

                if (((Literal)o).GetDatatype() != null) return -1;
                return 0;
            }
        }

        //[System.Serializable]
        public class IRI : Node
        {
            public IRI(string iri)
            {
                this["type"] = "IRI";
                this["value"] = iri;
            }

            public override bool IsLiteral()
            {
                return false;
            }

            public override bool IsIRI()
            {
                return true;
            }

            public override bool IsBlankNode()
            {
                return false;
            }
        }

        //[System.Serializable]
        public class BlankNode : Node
        {
            public BlankNode(string attribute)
            {
                this["type"] = "blank node";
                this["value"] = attribute;
            }

            public override bool IsLiteral()
            {
                return false;
            }

            public override bool IsIRI()
            {
                return false;
            }

            public override bool IsBlankNode()
            {
                return true;
            }
        }
    }
}