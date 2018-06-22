using System.Collections.Generic;
using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace JsonLD.Impl
{
    public class TurtleTripleCallback : IJsonLdTripleCallback
    {
        private const int MaxLineLength = 160;

        private const int TabSpaces = 4;

        private const string ColsKey = "..cols..";

        internal readonly IDictionary<string, string> availableNamespaces = new _Dictionary_32
            ();

        internal ICollection<string> usedNamespaces;

        // this shouldn't be a
        // valid iri/bnode i
        // hope!
        // TODO: fill with default namespaces
        public virtual object Call(RdfDataset dataset)
        {
            foreach (var e in dataset.GetNamespaces().GetEnumerableSelf
                ())
                availableNamespaces[e.Value] = e.Key;
            usedNamespaces = new HashSet<string>();
            var refs = new JObject();
            var ttl = new JObject();
            foreach (var graphName in dataset.Keys)
            {
                var localGraphName = graphName;
                var triples = dataset.GetQuads(localGraphName);
                if ("@default".Equals(localGraphName)) localGraphName = null;

                // http://www.w3.org/TR/turtle/#unlabeled-bnodes
                // TODO: implement nesting for unlabled nodes
                // map of what the output should look like
                // subj (or [ if bnode) > pred > obj
                // > obj (set ref if IRI)
                // > pred > obj (set ref if bnode)
                // subj > etc etc etc
                // subjid -> [ ref, ref, ref ]
                var prevSubject = string.Empty;
                var prevPredicate = string.Empty;
                JObject thisSubject = null;
                JArray thisPredicate = null;
                foreach (var triple in triples)
                {
                    var subject = triple.GetSubject().GetValue();
                    var predicate = triple.GetPredicate().GetValue();
                    if (prevSubject.Equals(subject))
                    {
                        if (prevPredicate.Equals(predicate))
                        {
                        }
                        else
                        {
                            // nothing to do
                            // new predicate
                            if (thisSubject.ContainsKey(predicate))
                            {
                                thisPredicate = (JArray)thisSubject[predicate];
                            }
                            else
                            {
                                thisPredicate = new JArray();
                                thisSubject[predicate] = thisPredicate;
                            }

                            prevPredicate = predicate;
                        }
                    }
                    else
                    {
                        // new subject
                        if (ttl.ContainsKey(subject))
                        {
                            thisSubject = (JObject)ttl[subject];
                        }
                        else
                        {
                            thisSubject = new JObject();
                            ttl[subject] = thisSubject;
                        }

                        if (thisSubject.ContainsKey(predicate))
                        {
                            thisPredicate = (JArray)thisSubject[predicate];
                        }
                        else
                        {
                            thisPredicate = new JArray();
                            thisSubject[predicate] = thisPredicate;
                        }

                        prevSubject = subject;
                        prevPredicate = predicate;
                    }

                    if (triple.GetObject().IsLiteral())
                    {
                        thisPredicate.Add(triple.GetObject());
                    }
                    else
                    {
                        var o = triple.GetObject().GetValue();
                        if (o.StartsWith("_:"))
                        {
                            // add ref to o
                            if (!refs.ContainsKey(o)) refs[o] = new JArray();
                            ((JArray)refs[o]).Add(thisPredicate);
                        }

                        thisPredicate.Add(o);
                    }
                }
            }

            var collections = new JObject();
            var subjects = new JArray(ttl.GetKeys());

            // find collections
            foreach (string subj in subjects)
            {
                var preds = (JObject)ttl[subj];
                if (preds != null && preds.ContainsKey(JsonLdConsts.RdfFirst))
                {
                    var col = new JArray();
                    collections[subj] = col;
                    while (true)
                    {
                        var first = (JArray)Collections.Remove(preds, JsonLdConsts.RdfFirst);
                        var o = first[0];
                        col.Add(o);

                        // refs
                        if (refs.ContainsKey((string)o))
                        {
                            ((JArray)refs[(string)o]).Remove(first);
                            ((JArray)refs[(string)o]).Add(col);
                        }

                        var next = (string)Collections.Remove(preds, JsonLdConsts.RdfRest)[0
                        ];
                        if (JsonLdConsts.RdfNil.Equals(next)) break;

                        // if collections already contains a value for "next", add
                        // it to this col and break out
                        if (collections.ContainsKey(next))
                        {
                            Collections.AddAll(col, (JArray)Collections.Remove(collections, next));
                            break;
                        }

                        preds = (JObject)Collections.Remove(ttl, next);
                        Collections.Remove(refs, next);
                    }
                }
            }

            // process refs (nesting referenced bnodes if only one reference to them
            // in the whole graph)
            foreach (var id in refs.GetKeys())
            {
                // skip items if there is more than one reference to them in the
                // graph
                if (((JArray)refs[id]).Count > 1) continue;

                // otherwise embed them into the referenced location
                var @object = Collections.Remove(ttl, id);
                if (collections.ContainsKey(id))
                {
                    @object = new JObject();
                    var tmp = new JArray();
                    tmp.Add(Collections.Remove(collections, id));
                    ((JObject)@object)[ColsKey] = tmp;
                }

                var predicate = (JArray)refs[id][0];

                // replace the one bnode ref with the object
                predicate[predicate.LastIndexOf(id)] = @object;
            }

            // replace the rest of the collections
            foreach (var id_1 in collections.GetKeys())
            {
                var subj_1 = (JObject)ttl[id_1];
                if (!subj_1.ContainsKey(ColsKey)) subj_1[ColsKey] = new JArray();
                ((JArray)subj_1[ColsKey]).Add(collections[id_1]);
            }

            // build turtle output
            var output = GenerateTurtle(ttl, 0, 0, false);
            var prefixes = string.Empty;
            foreach (var prefix in usedNamespaces)
            {
                var name = availableNamespaces[prefix];
                prefixes += "@prefix " + name + ": <" + prefix + "> .\n";
            }

            return (string.Empty.Equals(prefixes) ? string.Empty : prefixes + "\n") + output;
        }

        private string GenerateObject(object @object, string sep, bool hasNext, int indentation, int lineLength)
        {
            var rval = string.Empty;
            string obj;
            if (@object is string)
            {
                obj = GetURI((string)@object);
            }
            else
            {
                if (@object is RdfDataset.Literal)
                {
                    obj = ((RdfDataset.Literal)@object).GetValue();
                    var lang = ((RdfDataset.Literal)@object).GetLanguage();
                    var dt = ((RdfDataset.Literal)@object).GetDatatype();
                    if (lang != null)
                    {
                        obj = "\"" + obj + "\"";
                        obj += "@" + lang;
                    }
                    else
                    {
                        if (dt != null)
                        {
                            // TODO: this probably isn't an exclusive list of all the
                            // datatype literals that can be represented as native types
                            if (!(JsonLdConsts.XsdDouble.Equals(dt) || JsonLdConsts.XsdInteger.Equals(dt) ||
                                  JsonLdConsts.XsdFloat.Equals(dt) || JsonLdConsts.XsdBoolean.Equals(dt)))
                            {
                                obj = "\"" + obj + "\"";
                                if (!JsonLdConsts.XsdString.Equals(dt)) obj += "^^" + GetURI(dt);
                            }
                        }
                        else
                        {
                            obj = "\"" + obj + "\"";
                        }
                    }
                }
                else
                {
                    // must be an object
                    var tmp = new JObject();
                    tmp["_:x"] = (JObject)@object;
                    obj = GenerateTurtle(tmp, indentation + 1, lineLength, true);
                }
            }

            var idxofcr = obj.IndexOf("\n");

            // check if output will fix in the max line length (factor in comma if
            // not the last item, current line length and length to the next CR)
            if ((hasNext ? 1 : 0) + lineLength + (idxofcr != -1 ? idxofcr : obj.Length) > MaxLineLength)
            {
                rval += "\n" + Tabs(indentation + 1);
                lineLength = (indentation + 1) * TabSpaces;
            }

            rval += obj;
            if (idxofcr != -1)
                lineLength += obj.Length - obj.LastIndexOf("\n");
            else
                lineLength += obj.Length;
            if (hasNext)
            {
                rval += sep;
                lineLength += sep.Length;
                if (lineLength < MaxLineLength)
                {
                    rval += " ";
                    lineLength++;
                }
                else
                {
                    rval += "\n";
                }
            }

            return rval;
        }

        private string GenerateTurtle(JObject ttl, int indentation, int lineLength, bool isObject)
        {
            var rval = string.Empty;
            var subjIter = ttl.GetKeys().GetEnumerator();
            while (subjIter.MoveNext())
            {
                var subject = subjIter.Current;
                var subjval = (JObject)ttl[subject];

                // boolean isBlankNode = subject.startsWith("_:");
                var hasOpenBnodeBracket = false;
                if (subject.StartsWith("_:"))
                {
                    // only open blank node bracket the node doesn't contain any
                    // collections
                    if (!subjval.ContainsKey(ColsKey))
                    {
                        rval += "[ ";
                        lineLength += 2;
                        hasOpenBnodeBracket = true;
                    }

                    // TODO: according to http://www.rdfabout.com/demo/validator/
                    // 1) collections as objects cannot contain any predicates other
                    // than rdf:first and rdf:rest
                    // 2) collections cannot be surrounded with [ ]
                    // check for collection
                    if (subjval.ContainsKey(ColsKey))
                    {
                        var collections = (JArray)Collections.Remove(subjval, ColsKey);
                        foreach (var collection in collections)
                        {
                            rval += "( ";
                            lineLength += 2;

                            var objIter = ((JArray)collection).Children().GetEnumerator();
                            while (objIter.MoveNext())
                            {
                                var @object = objIter.Current;
                                rval += GenerateObject(@object,
                                                       string.Empty,
                                                       objIter.MoveNext(),
                                                       indentation,
                                                       lineLength
                                );
                                lineLength = rval.Length - rval.LastIndexOf("\n");
                            }

                            rval += " ) ";
                            lineLength += 3;
                        }
                    }
                }
                else
                {
                    // check for blank node
                    rval += GetURI(subject) + " ";
                    lineLength += subject.Length + 1;
                }

                var predIter = ttl[subject].GetKeys().GetEnumerator();
                while (predIter.MoveNext())
                {
                    var predicate = predIter.Current;
                    rval += GetURI(predicate) + " ";
                    lineLength += predicate.Length + 1;
                    var objIter = ((JArray)ttl[subject][predicate]).Children().GetEnumerator();
                    while (objIter.MoveNext())
                    {
                        var @object = objIter.Current;
                        rval += GenerateObject(@object, ",", objIter.MoveNext(), indentation, lineLength);
                        lineLength = rval.Length - rval.LastIndexOf("\n");
                    }

                    if (predIter.MoveNext())
                    {
                        rval += " ;\n" + Tabs(indentation + 1);
                        lineLength = (indentation + 1) * TabSpaces;
                    }
                }

                if (hasOpenBnodeBracket) rval += " ]";
                if (!isObject)
                {
                    rval += " .\n";
                    if (subjIter.MoveNext()) rval += "\n";
                }
            }

            return rval;
        }

        // TODO: Assert (TAB_SPACES == 4) otherwise this needs to be edited, and
        // should fail to compile
        private string Tabs(int tabs)
        {
            var rval = string.Empty;
            for (var i = 0; i < tabs; i++) rval += "    ";

            // using spaces for tabs
            return rval;
        }

        /// <summary>
        ///     checks the URI for a prefix, and if one is found, set used prefixes to
        ///     true
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private string GetURI(string uri)
        {
            // check for bnode
            if (uri.StartsWith("_:")) return uri;
            foreach (var prefix in availableNamespaces.Keys)
                if (uri.StartsWith(prefix))
                {
                    usedNamespaces.Add(prefix);

                    // return the prefixed URI
                    return availableNamespaces[prefix] + ":" + JavaCompat.Substring(uri, prefix.Length);
                }

            // return the full URI
            return "<" + uri + ">";
        }

        private sealed class _Dictionary_32 : Dictionary<string, string>
        {
        }
    }
}