using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonLD.Util;
using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    /// <summary>
    ///     A helper class which still stores all the values in a map but gives member
    ///     variables easily access certain keys
    /// </summary>
    /// <author>tristan</author>
    //[System.Serializable]
    public class Context : JObject, ICloneable
    {
        public Context()
            : this(new JsonLdOptions())
        {
        }

        public Context(JsonLdOptions options)
        {
            Init(options);
        }

        public Context(JObject map, JsonLdOptions options)
            : base(map)
        {
            Init(options);
        }

        public Context(JObject map)
            : base(map)
        {
            Init(new JsonLdOptions());
        }

        public Context(JToken context, JsonLdOptions opts)
            : base(context as JObject)
        {
            Init(opts);
        }

        public JObject Inverse { get; set; }

        private JsonLdOptions Options { get; set; }

        private JObject TermDefinitions { get; set; }

        public object Clone()
        {
            return new Context(DeepClone(), Options) {TermDefinitions = (JObject)TermDefinitions.DeepClone()};
        }

        // TODO: load remote context
        private void Init(JsonLdOptions options)
        {
            Options = options;
            if (options.GetBase() != null) this["@base"] = options.GetBase();
            TermDefinitions = new JObject();
        }

        /// <summary>
        ///     Value Compaction Algorithm
        ///     http://json-ld.org/spec/latest/json-ld-api/#value-compaction
        /// </summary>
        /// <param name="activeProperty"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual JToken CompactValue(string activeProperty, JObject value)
        {
            var dict = (IDictionary<string, JToken>)value;

            // 1)
            var numberMembers = value.Count;

            // 2)
            if (dict.ContainsKey("@index") && "@index".Equals(GetContainer(activeProperty
                                                              )))
                numberMembers--;

            // 3)
            if (numberMembers > 2) return value;

            // 4)
            var typeMapping = GetTypeMapping(activeProperty);
            var languageMapping = GetLanguageMapping(activeProperty);
            if (dict.ContainsKey("@id"))
            {
                // 4.1)
                if (numberMembers == 1 && "@id".Equals(typeMapping)) return CompactIri((string)value["@id"]);

                // 4.2)
                if (numberMembers == 1 && "@vocab".Equals(typeMapping)) return CompactIri((string)value["@id"], true);

                // 4.3)
                return value;
            }

            var valueValue = value["@value"];

            // 5)
            if (dict.ContainsKey("@type") && value["@type"].SafeCompare(typeMapping)) return valueValue;

            // 6)
            if (dict.ContainsKey("@language"))
                if (value["@language"].SafeCompare(languageMapping) || value["@language"].SafeCompare(this["@language"]))
                    return valueValue;

            // 7)
            if (numberMembers == 1 && (valueValue.Type != JTokenType.String || !((IDictionary<string, JToken>)this).ContainsKey("@language") ||
                                       GetTermDefinition(activeProperty).ContainsKey("@language") && languageMapping == null))
                return valueValue;

            // 8)
            return value;
        }
        
        public static async Task<Context> ParseAsync(JToken localContext, JsonLdOptions options)
        {
            var ctx = new Context(options);

            return await ctx.ParseAsync(localContext);
        }

        /// <summary>
        ///     Context Processing Algorithm
        ///     http://json-ld.org/spec/latest/json-ld-api/#context-processing-algorithms
        /// </summary>
        /// <param name="localContext"></param>
        /// <param name="remoteContexts"></param>
        /// <returns></returns>
        /// <exception cref="JsonLdError"></exception>
        public virtual async Task<Context> ParseAsync(JToken localContext, List<string> remoteContexts = null)
        {
            if (remoteContexts == null) remoteContexts = new List<string>();

            // 1. Initialize result to the result of cloning active context.
            var result = (Context)Clone();

            // TODO: clone?
            // 2)
            if (!(localContext is JArray))
            {
                var temp = localContext;
                localContext = new JArray();
                ((JArray)localContext).Add(temp);
            }

            // 3)
            foreach (var context in (JArray)localContext)
            {
                var eachContext = context;

                // 3.1)
                if (eachContext.Type == JTokenType.Null)
                {
                    result = new Context(Options);
                    continue;
                }

                if (eachContext is Context)
                {
                    result = (Context)(eachContext as Context).Clone();
                }
                else
                {
                    // 3.2)
                    if (eachContext.Type == JTokenType.String)
                    {
                        var uri = (string)result["@base"];
                        uri = Url.Resolve(uri, (string)eachContext);

                        // 3.2.2
                        if (remoteContexts.Contains(uri)) throw new JsonLdError(JsonLdError.Error.RecursiveContextInclusion, uri);
                        remoteContexts.Add(uri);

                        // 3.2.3: Dereference context
                        RemoteDocument rd;
                        try
                        {
                            rd = await Options.documentLoader.LoadDocumentAsync(uri);
                        }
                        catch (JsonLdError err)
                        {
                            if (err.Message.StartsWith(JsonLdError.Error.LoadingDocumentFailed.ToString()))
                                throw new JsonLdError(JsonLdError.Error.LoadingRemoteContextFailed);
                            throw;
                        }

                        var remoteContext = rd.document;
                        if (!(remoteContext is JObject) || !((JObject)remoteContext).ContainsKey("@context"))
                            throw new JsonLdError(JsonLdError.Error.InvalidRemoteContext, eachContext);
                        eachContext = ((JObject)remoteContext)["@context"];

                        // 3.2.4
                        result = await result.ParseAsync(eachContext, remoteContexts);

                        // 3.2.5
                        continue;
                    }

                    if (!(eachContext is JObject)) throw new JsonLdError(JsonLdError.Error.InvalidLocalContext, eachContext);
                }

                // 3.4
                if (remoteContexts.IsEmpty() && ((JObject)eachContext).ContainsKey
                        ("@base"))
                {
                    var value = eachContext["@base"];
                    if (value.IsNull())
                    {
                        Collections.Remove(result, "@base");
                    }
                    else
                    {
                        if (value.Type == JTokenType.String)
                            if (JsonLdUtils.IsAbsoluteIri((string)value))
                            {
                                result["@base"] = value;
                            }
                            else
                            {
                                var baseUri = (string)result["@base"];
                                if (!JsonLdUtils.IsAbsoluteIri(baseUri)) throw new JsonLdError(JsonLdError.Error.InvalidBaseIri, baseUri);
                                result["@base"] = Url.Resolve(baseUri, (string)value);
                            }
                        else
                            throw new JsonLdError(JsonLdError.Error.InvalidBaseIri, "@base must be a string");
                    }
                }

                // 3.5
                if (((JObject)eachContext).ContainsKey("@vocab"))
                {
                    var value = eachContext["@vocab"];
                    if (value.IsNull())
                    {
                        Collections.Remove(result, "@vocab");
                    }
                    else
                    {
                        if (value.Type == JTokenType.String)
                            if (JsonLdUtils.IsAbsoluteIri((string)value))
                                result["@vocab"] = value;
                            else
                                throw new JsonLdError(JsonLdError.Error.InvalidVocabMapping,
                                                      "@value must be an absolute IRI"
                                );
                        else
                            throw new JsonLdError(JsonLdError.Error.InvalidVocabMapping,
                                                  "@vocab must be a string or null"
                            );
                    }
                }

                // 3.6
                if (((JObject)eachContext).ContainsKey("@language"))
                {
                    var value = ((JObject)eachContext)["@language"];
                    if (value.IsNull())
                    {
                        Collections.Remove(result, "@language");
                    }
                    else
                    {
                        if (value.Type == JTokenType.String)
                            result["@language"] = ((string)value).ToLower();
                        else
                            throw new JsonLdError(JsonLdError.Error.InvalidDefaultLanguage, value);
                    }
                }

                // 3.7
                IDictionary<string, bool> defined = new Dictionary<string, bool>();
                foreach (var key in eachContext.GetKeys())
                {
                    if ("@base".Equals(key) || "@vocab".Equals(key) || "@language".Equals(key)) continue;
                    result.CreateTermDefinition((JObject)eachContext, key, defined);
                }
            }

            return result;
        }

        /// <summary>
        ///     Create Term Definition Algorithm
        ///     http://json-ld.org/spec/latest/json-ld-api/#create-term-definition
        /// </summary>
        /// <param name="context"></param>
        /// <param name="term"></param>
        /// <param name="defined"></param>
        /// <exception cref="JsonLdError">JsonLdError</exception>
        private void CreateTermDefinition(JObject context, string term, IDictionary<string, bool> defined)
        {
            if (defined.ContainsKey(term))
            {
                if (defined[term]) return;
                throw new JsonLdError(JsonLdError.Error.CyclicIriMapping, term);
            }

            defined[term] = false;
            if (JsonLdUtils.IsKeyword(term)) throw new JsonLdError(JsonLdError.Error.KeywordRedefinition, term);
            Collections.Remove(TermDefinitions, term);
            var value = context[term];
            if (value.IsNull() || value is JObject && ((IDictionary<string, JToken>)value
                                                      ).ContainsKey("@id") && ((IDictionary<string, JToken>)value)["@id"].IsNull())
            {
                TermDefinitions[term] = null;
                defined[term] = true;
                return;
            }

            if (value.Type == JTokenType.String)
            {
                value = new JObject {["@id"] = value};
            }

            if (!(value is JObject)) throw new JsonLdError(JsonLdError.Error.InvalidTermDefinition, value);

            // casting the value so it doesn't have to be done below everytime
            var val = (JObject)value;

            // 9) create a new term definition
            var definition = new JObject();

            // 10)
            if (val.ContainsKey("@type"))
            {
                if (val["@type"].Type != JTokenType.String) throw new JsonLdError(JsonLdError.Error.InvalidTypeMapping, val["@type"]);
                var type = (string)val["@type"];
                try
                {
                    type = ExpandIri((string)val["@type"], false, true, context, defined);
                }
                catch (JsonLdError error)
                {
                    if (error.GetType() != JsonLdError.Error.InvalidIriMapping) throw;
                    throw new JsonLdError(JsonLdError.Error.InvalidTypeMapping, type);
                }

                // TODO: fix check for absoluteIri (blank nodes shouldn't count, at
                // least not here!)
                if ("@id".Equals(type) || "@vocab".Equals(type) || !type.StartsWith("_:") && JsonLdUtils
                        .IsAbsoluteIri(type))
                    definition["@type"] = type;
                else
                    throw new JsonLdError(JsonLdError.Error.InvalidTypeMapping, type);
            }

            // 11)
            if (val.ContainsKey("@reverse"))
            {
                if (val.ContainsKey("@id")) throw new JsonLdError(JsonLdError.Error.InvalidReverseProperty, val);
                if (val["@reverse"].Type != JTokenType.String)
                    throw new JsonLdError(JsonLdError.Error.InvalidIriMapping,
                                          "Expected String for @reverse value. got "
                                          + (val["@reverse"].IsNull() ? "null" : val["@reverse"].GetType().ToString()));
                var reverse = ExpandIri((string)val["@reverse"],
                                        false,
                                        true,
                                        context,
                                        defined
                );
                if (!JsonLdUtils.IsAbsoluteIri(reverse))
                    throw new JsonLdError(JsonLdError.Error.InvalidIriMapping,
                                          "Non-absolute @reverse IRI: "
                                          + reverse);
                definition["@id"] = reverse;
                if (val.ContainsKey("@container"))
                {
                    var container = (string)val["@container"];
                    if (container == null || "@set".Equals(container) || "@index".Equals(container))
                        definition["@container"] = container;
                    else
                        throw new JsonLdError(JsonLdError.Error.InvalidReverseProperty,
                                              "reverse properties only support set- and index-containers"
                        );
                }

                definition["@reverse"] = true;
                TermDefinitions[term] = definition;
                defined[term] = true;
                return;
            }

            // 12)
            definition["@reverse"] = false;

            // 13)
            if (!val["@id"].IsNull() && !val["@id"].SafeCompare(term))
            {
                if (val["@id"].Type != JTokenType.String)
                    throw new JsonLdError(JsonLdError.Error.InvalidIriMapping,
                                          "expected value of @id to be a string"
                    );
                var res = ExpandIri((string)val["@id"], false, true, context, defined);
                if (JsonLdUtils.IsKeyword(res) || JsonLdUtils.IsAbsoluteIri(res))
                {
                    if ("@context".Equals(res))
                        throw new JsonLdError(JsonLdError.Error.InvalidKeywordAlias,
                                              "cannot alias @context"
                        );
                    definition["@id"] = res;
                }
                else
                {
                    throw new JsonLdError(JsonLdError.Error.InvalidIriMapping,
                                          "resulting IRI mapping should be a keyword, absolute IRI or blank node"
                    );
                }
            }
            else
            {
                // 14)
                if (term.IndexOf(":", StringComparison.Ordinal) >= 0)
                {
                    var colIndex = term.IndexOf(":", StringComparison.Ordinal);
                    var prefix = JavaCompat.Substring(term, 0, colIndex);
                    var suffix = JavaCompat.Substring(term, colIndex + 1);
                    if (context.ContainsKey(prefix)) CreateTermDefinition(context, prefix, defined);
                    if (TermDefinitions.ContainsKey(prefix))
                        definition["@id"] = (string)((IDictionary<string, JToken>)TermDefinitions[prefix])["@id"] + suffix;
                    else
                        definition["@id"] = term;
                }
                else
                {
                    // 15)
                    if (this.ContainsKey("@vocab"))
                        definition["@id"] = (string)this["@vocab"] + term;
                    else
                        throw new JsonLdError(JsonLdError.Error.InvalidIriMapping,
                                              "relative term definition without vocab mapping"
                        );
                }
            }

            // 16)
            if (val.ContainsKey("@container"))
            {
                var container = (string)val["@container"];
                if (!"@list".Equals(container) && !"@set".Equals(container) && !"@index".Equals(container
                    ) && !"@language".Equals(container))
                    throw new JsonLdError(JsonLdError.Error.InvalidContainerMapping,
                                          "@container must be either @list, @set, @index, or @language"
                    );
                definition["@container"] = container;
            }

            // 17)
            if (val.ContainsKey("@language") && !val.ContainsKey("@type"))
                if (val["@language"].IsNull() || val["@language"].Type == JTokenType.String)
                {
                    var language = (string)val["@language"];
                    definition["@language"] = language?.ToLower();
                }
                else
                {
                    throw new JsonLdError(JsonLdError.Error.InvalidLanguageMapping,
                                          "@language must be a string or null"
                    );
                }

            // 18)
            TermDefinitions[term] = definition;
            defined[term] = true;
        }

        /// <summary>
        ///     IRI Expansion Algorithm
        ///     http://json-ld.org/spec/latest/json-ld-api/#iri-expansion
        /// </summary>
        /// <param name="value"></param>
        /// <param name="relative"></param>
        /// <param name="vocab"></param>
        /// <param name="context"></param>
        /// <param name="defined"></param>
        /// <returns></returns>
        /// <exception cref="JsonLdError">JsonLdError</exception>
        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        internal virtual string ExpandIri(string value, bool relative, bool vocab, JObject context, IDictionary<string, bool> defined)
        {
            // 1)
            if (value == null || JsonLdUtils.IsKeyword(value)) return value;

            // 2)
            if (context != null && context.ContainsKey(value) && defined.ContainsKey(value) && !defined[value])
                CreateTermDefinition(context, value, defined);

            // 3)
            if (vocab && TermDefinitions.ContainsKey(value))
            {
                var td = TermDefinitions[value];
                if (td.Type != JTokenType.Null)
                    return (string)((JObject)td)["@id"];
                return null;
            }

            // 4)
            var colIndex = value.IndexOf(":");
            if (colIndex >= 0)
            {
                // 4.1)
                var prefix = JavaCompat.Substring(value, 0, colIndex);
                var suffix = JavaCompat.Substring(value, colIndex + 1);

                // 4.2)
                if ("_".Equals(prefix) || suffix.StartsWith("//")) return value;

                // 4.3)
                if (context != null && context.ContainsKey(prefix) && (!defined.ContainsKey(prefix
                                                                       ) || defined[prefix] == false))
                    CreateTermDefinition(context, prefix, defined);

                // 4.4)
                if (TermDefinitions.ContainsKey(prefix))
                    return (string)((JObject)TermDefinitions[prefix])["@id"]
                           + suffix;

                // 4.5)
                return value;
            }

            // 5)
            if (vocab && this.ContainsKey("@vocab")) return (string)this["@vocab"] + value;

            // 6)
            if (relative) return Url.Resolve((string)this["@base"], value);

            if (context != null && JsonLdUtils.IsRelativeIri(value))
                throw new JsonLdError(JsonLdError.Error.InvalidIriMapping,
                                      "not an absolute IRI: "
                                      + value);

            // 7)
            return value;
        }

        /// <summary>
        ///     IRI Compaction Algorithm
        ///     http://json-ld.org/spec/latest/json-ld-api/#iri-compaction
        ///     Compacts an IRI or keyword into a term or prefix if it can be.
        /// </summary>
        /// <remarks>
        ///     IRI Compaction Algorithm
        ///     http://json-ld.org/spec/latest/json-ld-api/#iri-compaction
        ///     Compacts an IRI or keyword into a term or prefix if it can be. If the IRI
        ///     has an associated value it may be passed.
        /// </remarks>
        /// <param name="iri">the IRI to compact.</param>
        /// <param name="value">the value to check or null.</param>
        /// <param name="relativeTo">
        ///     options for how to compact IRIs: vocab: true to split after
        ///     false not to.
        /// </param>
        /// <param name="reverse">
        ///     true if a reverse property is being compacted, false if not.
        /// </param>
        /// <returns>the compacted term, prefix, keyword alias, or the original IRI.</returns>
        internal virtual string CompactIri(string iri, JToken value, bool relativeToVocab, bool reverse)
        {
            // 1)
            if (iri == null) return null;

            // 2)
            if (relativeToVocab && GetInverse().ContainsKey(iri))
            {
                // 2.1)
                var defaultLanguage = (string)this["@language"];
                if (defaultLanguage == null) defaultLanguage = "@none";

                // 2.2)
                var containers = new JArray();

                // 2.3)
                var typeLanguage = "@language";
                var typeLanguageValue = "@null";

                // 2.4)
                if (value is JObject && ((IDictionary<string, JToken>)value).ContainsKey("@index"
                    ))
                    containers.Add("@index");

                // 2.5)
                if (reverse)
                {
                    typeLanguage = "@type";
                    typeLanguageValue = "@reverse";
                    containers.Add("@set");
                }
                else
                {
                    // 2.6)
                    if (value is JObject && ((IDictionary<string, JToken>)value).ContainsKey("@list"))
                    {
                        // 2.6.1)
                        if (!((IDictionary<string, JToken>)value).ContainsKey("@index")) containers.Add("@list");

                        // 2.6.2)
                        var list = (JArray)((JObject)value)["@list"];

                        // 2.6.3)
                        var commonLanguage = list.Count == 0 ? defaultLanguage : null;
                        string commonType = null;

                        // 2.6.4)
                        foreach (var item in list)
                        {
                            // 2.6.4.1)
                            var itemLanguage = "@none";
                            var itemType = "@none";

                            // 2.6.4.2)
                            if (JsonLdUtils.IsValue(item))
                                if (((IDictionary<string, JToken>)item).ContainsKey("@language"))
                                {
                                    itemLanguage = (string)((JObject)item)["@language"];
                                }
                                else
                                {
                                    // 2.6.4.2.2)
                                    if (((IDictionary<string, JToken>)item).ContainsKey("@type"))
                                        itemType = (string)((JObject)item)["@type"];
                                    else
                                        itemLanguage = "@null";
                                }
                            else
                                itemType = "@id";

                            // 2.6.4.4)
                            if (commonLanguage == null)
                            {
                                commonLanguage = itemLanguage;
                            }
                            else
                            {
                                // 2.6.4.5)
                                if (!commonLanguage.Equals(itemLanguage) && JsonLdUtils.IsValue(item)) commonLanguage = "@none";
                            }

                            // 2.6.4.6)
                            if (commonType == null)
                            {
                                commonType = itemType;
                            }
                            else
                            {
                                // 2.6.4.7)
                                if (!commonType.Equals(itemType)) commonType = "@none";
                            }

                            // 2.6.4.8)
                            if ("@none".Equals(commonLanguage) && "@none".Equals(commonType)) break;
                        }

                        // 2.6.5)
                        commonLanguage = commonLanguage != null ? commonLanguage : "@none";

                        // 2.6.6)
                        commonType = commonType != null ? commonType : "@none";

                        // 2.6.7)
                        if (!"@none".Equals(commonType))
                        {
                            typeLanguage = "@type";
                            typeLanguageValue = commonType;
                        }
                        else
                        {
                            // 2.6.8)
                            typeLanguageValue = commonLanguage;
                        }
                    }
                    else
                    {
                        // 2.7)
                        // 2.7.1)
                        if (value is JObject && ((IDictionary<string, JToken>)value).ContainsKey("@value"
                            ))
                        {
                            // 2.7.1.1)
                            if (((IDictionary<string, JToken>)value).ContainsKey("@language") && !((IDictionary
                                                                                                          <string, JToken>)value)
                                    .ContainsKey("@index"))
                            {
                                containers.Add("@language");
                                typeLanguageValue = (string)((IDictionary<string, JToken>)value)["@language"];
                            }
                            else
                            {
                                // 2.7.1.2)
                                if (((IDictionary<string, JToken>)value).ContainsKey("@type"))
                                {
                                    typeLanguage = "@type";
                                    typeLanguageValue = (string)((IDictionary<string, JToken>)value)["@type"];
                                }
                            }
                        }
                        else
                        {
                            // 2.7.2)
                            typeLanguage = "@type";
                            typeLanguageValue = "@id";
                        }

                        // 2.7.3)
                        containers.Add("@set");
                    }
                }

                // 2.8)
                containers.Add("@none");

                // 2.9)
                if (typeLanguageValue == null) typeLanguageValue = "@null";

                // 2.10)
                var preferredValues = new JArray();

                // 2.11)
                if ("@reverse".Equals(typeLanguageValue)) preferredValues.Add("@reverse");

                // 2.12)
                if (("@reverse".Equals(typeLanguageValue) || "@id".Equals(typeLanguageValue)) &&
                    value is JObject && ((JObject)value).ContainsKey("@id"
                    ))
                {
                    // 2.12.1)
                    var result = CompactIri((string)((IDictionary<string, JToken>)value)["@id"
                                            ],
                                            null,
                                            true,
                                            true);
                    if (TermDefinitions.ContainsKey(result) && ((IDictionary<string, JToken>)TermDefinitions
                                                                       [result]).ContainsKey("@id") && ((IDictionary<string, JToken>)value)["@id"]
                        .SafeCompare
                            (((IDictionary<string, JToken>)TermDefinitions[result])["@id"]))
                    {
                        preferredValues.Add("@vocab");
                        preferredValues.Add("@id");
                    }
                    else
                    {
                        // 2.12.2)
                        preferredValues.Add("@id");
                        preferredValues.Add("@vocab");
                    }
                }
                else
                {
                    // 2.13)
                    preferredValues.Add(typeLanguageValue);
                }

                preferredValues.Add("@none");

                // 2.14)
                var term = SelectTerm(iri, containers, typeLanguage, preferredValues);

                // 2.15)
                if (term != null) return term;
            }

            // 3)
            if (relativeToVocab && this.ContainsKey("@vocab"))
            {
                // determine if vocab is a prefix of the iri
                var vocab = (string)this["@vocab"];

                // 3.1)
                if (iri.IndexOf(vocab) == 0 && !iri.Equals(vocab))
                {
                    // use suffix as relative iri if it is not a term in the
                    // active context
                    var suffix = JavaCompat.Substring(iri, vocab.Length);
                    if (!TermDefinitions.ContainsKey(suffix)) return suffix;
                }
            }

            // 4)
            string compactIRI = null;

            // 5)
            foreach (var term_1 in TermDefinitions.GetKeys())
            {
                var termDefinitionToken = TermDefinitions[term_1];

                // 5.1)
                if (term_1.Contains(":")) continue;

                // 5.2)
                if (termDefinitionToken.Type == JTokenType.Null) continue;
                var termDefinition = (JObject)termDefinitionToken;
                if (termDefinition["@id"].SafeCompare(iri) || !iri.StartsWith
                        ((string)termDefinition["@id"]))
                    continue;

                // 5.3)
                var candidate = term_1 + ":" + JavaCompat.Substring(iri,
                                                                    ((string)termDefinition
                                                                        ["@id"]).Length);

                // 5.4)
                if ((compactIRI == null || JsonLdUtils.CompareShortestLeast(candidate,
                                                                            compactIRI
                     ) < 0) && (!TermDefinitions.ContainsKey(candidate) || ((IDictionary<
                                                                                   string, JToken>)TermDefinitions[candidate])["@id"]
                                .SafeCompare(iri) && value.IsNull()))
                    compactIRI = candidate;
            }

            // 6)
            if (compactIRI != null) return compactIRI;

            // 7)
            if (!relativeToVocab) return Url.RemoveBase(this["@base"], iri);

            // 8)
            return iri;
        }

        internal virtual string CompactIri(string iri, bool relativeToVocab)
        {
            return CompactIri(iri, null, relativeToVocab, false);
        }

        internal virtual string CompactIri(string iri)
        {
            return CompactIri(iri, null, false, false);
        }

        /// <summary>
        ///     Inverse Context Creation
        ///     http://json-ld.org/spec/latest/json-ld-api/#inverse-context-creation
        ///     Generates an inverse context for use in the compaction algorithm, if not
        ///     already generated for the given active context.
        /// </summary>
        /// <remarks>
        ///     Inverse Context Creation
        ///     http://json-ld.org/spec/latest/json-ld-api/#inverse-context-creation
        ///     Generates an inverse context for use in the compaction algorithm, if not
        ///     already generated for the given active context.
        /// </remarks>
        /// <returns>the inverse context.</returns>
        public virtual JObject GetInverse()
        {
            // lazily create inverse
            if (Inverse != null) return Inverse;

            // 1)
            Inverse = new JObject();

            // 2)
            var defaultLanguage = (string)this["@language"];
            if (defaultLanguage == null) defaultLanguage = "@none";

            // create term selections for each mapping in the context, ordererd by
            // shortest and then lexicographically least
            var terms = new JArray(TermDefinitions.GetKeys());
            terms.SortInPlace(new _IComparer_794());
            foreach (string term in terms)
            {
                var definitionToken = TermDefinitions[term];

                // 3.1)
                if (definitionToken.Type == JTokenType.Null) continue;

                var definition = (JObject)TermDefinitions[term];

                // 3.2)
                var container = (string)definition["@container"];
                if (container == null) container = "@none";

                // 3.3)
                var iri = (string)definition["@id"];

                // 3.4 + 3.5)
                var containerMap = (JObject)Inverse[iri];
                if (containerMap == null)
                {
                    containerMap = new JObject();
                    Inverse[iri] = containerMap;
                }

                // 3.6 + 3.7)
                var typeLanguageMap = (JObject)containerMap[container];
                if (typeLanguageMap == null)
                {
                    typeLanguageMap = new JObject();
                    typeLanguageMap["@language"] = new JObject();
                    typeLanguageMap["@type"] = new JObject();
                    containerMap[container] = typeLanguageMap;
                }

                // 3.8)
                if (definition["@reverse"].SafeCompare(true))
                {
                    var typeMap = (JObject)typeLanguageMap
                        ["@type"];
                    if (!typeMap.ContainsKey("@reverse")) typeMap["@reverse"] = term;
                }
                else
                {
                    // 3.9)
                    if (definition.ContainsKey("@type"))
                    {
                        var typeMap = (JObject)typeLanguageMap["@type"];
                        if (!typeMap.ContainsKey((string)definition["@type"])) typeMap[(string)definition["@type"]] = term;
                    }
                    else
                    {
                        // 3.10)
                        if (definition.ContainsKey("@language"))
                        {
                            var languageMap = (JObject)typeLanguageMap
                                ["@language"];
                            var language = (string)definition["@language"];
                            if (language == null) language = "@null";
                            if (!languageMap.ContainsKey(language)) languageMap[language] = term;
                        }
                        else
                        {
                            // 3.11)
                            // 3.11.1)
                            var languageMap = (JObject)typeLanguageMap
                                ["@language"];

                            // 3.11.2)
                            if (!languageMap.ContainsKey("@language")) languageMap["@language"] = term;

                            // 3.11.3)
                            if (!languageMap.ContainsKey("@none")) languageMap["@none"] = term;

                            // 3.11.4)
                            var typeMap = (JObject)typeLanguageMap
                                ["@type"];

                            // 3.11.5)
                            if (!typeMap.ContainsKey("@none")) typeMap["@none"] = term;
                        }
                    }
                }
            }

            // 4)
            return Inverse;
        }

        /// <summary>
        ///     Term Selection
        ///     http://json-ld.org/spec/latest/json-ld-api/#term-selection
        ///     This algorithm, invoked via the IRI Compaction algorithm, makes use of an
        ///     active context's inverse context to find the term that is best used to
        ///     compact an IRI.
        /// </summary>
        /// <remarks>
        ///     Term Selection
        ///     http://json-ld.org/spec/latest/json-ld-api/#term-selection
        ///     This algorithm, invoked via the IRI Compaction algorithm, makes use of an
        ///     active context's inverse context to find the term that is best used to
        ///     compact an IRI. Other information about a value associated with the IRI
        ///     is given, including which container mappings and which type mapping or
        ///     language mapping would be best used to express the value.
        /// </remarks>
        /// <returns>the selected term.</returns>
        private string SelectTerm(string iri, JArray containers, string typeLanguage, JArray preferredValues)
        {
            var inv = GetInverse();

            // 1)
            var containerMap = (JObject)inv[iri];

            // 2)
            foreach (string container in containers)
            {
                // 2.1)
                if (!containerMap.ContainsKey(container)) continue;

                // 2.2)
                var typeLanguageMap = (JObject)containerMap
                    [container];

                // 2.3)
                var valueMap = (JObject)typeLanguageMap
                    [typeLanguage];

                // 2.4 )
                foreach (string item in preferredValues)
                {
                    // 2.4.1
                    if (!valueMap.ContainsKey(item)) continue;

                    // 2.4.2
                    return (string)valueMap[item];
                }
            }

            // 3)
            return null;
        }

        /// <summary>Retrieve container mapping.</summary>
        /// <remarks>Retrieve container mapping.</remarks>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual string GetContainer(string property)
        {
            // TODO(sblom): Do java semantics of get() on a Map return null if property is null?
            if (property == null) return null;

            if ("@graph".Equals(property)) return "@set";
            if (JsonLdUtils.IsKeyword(property)) return property;
            var td = (JObject)TermDefinitions[property
            ];
            if (td == null) return null;
            return (string)td["@container"];
        }

        public virtual bool IsReverseProperty(string property)
        {
            if (property == null) return false;
            var td = (JObject)TermDefinitions[property];
            if (td == null) return false;
            var reverse = td["@reverse"];
            return !reverse.IsNull() && (bool)reverse;
        }

        private string GetTypeMapping(string property)
        {
            if (property == null) return null;
            var td = TermDefinitions[property];
            if (td.IsNull()) return null;
            return (string)((JObject)td)["@type"];
        }

        private string GetLanguageMapping(string property)
        {
            if (property == null) return null;
            var td = (JObject)TermDefinitions[property];
            if (td == null) return null;
            return (string)td["@language"];
        }

        internal virtual JObject GetTermDefinition(string key)
        {
            return (JObject)TermDefinitions[key];
        }

        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        public virtual JToken ExpandValue(string activeProperty, JToken value)
        {
            var rval = new JObject();
            var td = GetTermDefinition(activeProperty);

            // 1)
            if (td != null && td["@type"].SafeCompare("@id"))
            {
                // TODO: i'm pretty sure value should be a string if the @type is
                // @id
                rval["@id"] = ExpandIri((string)value, true, false, null, null);
                return rval;
            }

            // 2)
            if (td != null && td["@type"].SafeCompare("@vocab"))
            {
                // TODO: same as above
                rval["@id"] = ExpandIri((string)value, true, true, null, null);
                return rval;
            }

            // 3)
            rval["@value"] = value;

            // 4)
            if (td != null && td.ContainsKey("@type"))
            {
                rval["@type"] = td["@type"];
            }
            else
            {
                // 5)
                if (value.Type == JTokenType.String)
                    if (td != null && td.ContainsKey("@language"))
                    {
                        var lang = (string)td["@language"];
                        if (lang != null) rval["@language"] = lang;
                    }
                    else
                    {
                        // 5.2)
                        if (!this["@language"].IsNull()) rval["@language"] = this["@language"];
                    }
            }

            return rval;
        }

        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        public virtual JObject GetContextValue(string activeProperty, string @string)
        {
            throw new JsonLdError(JsonLdError.Error.NotImplemented,
                                  "getContextValue is only used by old code so far and thus isn't implemented"
            );
        }

        public virtual JObject Serialize()
        {
            var ctx = new JObject();
            if (!this["@base"].IsNull() && !this["@base"].SafeCompare(Options.GetBase())) ctx["@base"] = this["@base"];
            if (!this["@language"].IsNull()) ctx["@language"] = this["@language"];
            if (!this["@vocab"].IsNull()) ctx["@vocab"] = this["@vocab"];
            foreach (var term in TermDefinitions.GetKeys())
            {
                var definition = (JObject)TermDefinitions[term];
                if (definition["@language"].IsNull() && definition["@container"].IsNull() && definition
                        ["@type"].IsNull() && (definition["@reverse"].IsNull() ||
                                               definition["@reverse"].Type == JTokenType.Boolean && (bool)definition["@reverse"] == false))
                {
                    var cid = CompactIri((string)definition["@id"]);
                    ctx[term] = term.Equals(cid) ? (string)definition["@id"] : cid;
                }
                else
                {
                    var defn = new JObject();
                    var cid = CompactIri((string)definition["@id"]);
                    var reverseProperty = definition["@reverse"].SafeCompare(true);
                    if (!(term.Equals(cid) && !reverseProperty)) defn[reverseProperty ? "@reverse" : "@id"] = cid;
                    var typeMapping = (string)definition["@type"];
                    if (typeMapping != null)
                        defn["@type"] = JsonLdUtils.IsKeyword(typeMapping) ? typeMapping : CompactIri(typeMapping, true);
                    if (!definition["@container"].IsNull()) defn["@container"] = definition["@container"];
                    var lang = definition["@language"];
                    if (!definition["@language"].IsNull()) defn["@language"] = lang.SafeCompare(false) ? null : lang;
                    ctx[term] = defn;
                }
            }

            var rval = new JObject();
            if (!(ctx == null || ctx.IsEmpty())) rval["@context"] = ctx;
            return rval;
        }

        private sealed class _IComparer_794 : IComparer<JToken>
        {
            public int Compare(JToken a, JToken b)
            {
                return JsonLdUtils.CompareShortestLeast((string)a, (string)b);
            }
        }
    }
}