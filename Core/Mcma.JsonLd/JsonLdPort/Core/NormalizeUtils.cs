using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    internal class NormalizeUtils
    {
        private readonly IDictionary<string, IDictionary<string, object>> bnodes;
        private readonly UniqueNamer namer;

        private readonly JsonLdOptions options;

        private readonly IList<RdfDataset.Quad> quads;

        public NormalizeUtils(IList<RdfDataset.Quad> quads,
                              IDictionary<string, IDictionary<string, object>> bnodes,
                              UniqueNamer
                                  namer,
                              JsonLdOptions options)
        {
            this.options = options;
            this.quads = quads;
            this.bnodes = bnodes;
            this.namer = namer;
        }

        // generates unique and duplicate hashes for bnodes
        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        public virtual object HashBlankNodes(IEnumerable<string> unnamed_)
        {
#if !PORTABLE
            IList<string> unnamed = new List<string>(unnamed_);
            IList<string> nextUnnamed = new List<string>();
            IDictionary<string, IList<string>> duplicates = new Dictionary<string, IList<string
            >>();
            IDictionary<string, string> unique = new Dictionary<string, string>();

            // NOTE: not using the same structure as javascript here to avoid
            // possible stack overflows
            // hash quads for each unnamed bnode
            for (var hui = 0;; hui++)
            {
                if (hui == unnamed.Count)
                {
                    // done, name blank nodes
                    var named = false;
                    IList<string> hashes = new List<string>(unique.Keys);
                    hashes.SortInPlace();
                    foreach (var hash in hashes)
                    {
                        var bnode = unique[hash];
                        namer.GetName(bnode);
                        named = true;
                    }

                    // continue to hash bnodes if a bnode was assigned a name
                    if (named)
                    {
                        // this resets the initial variables, so it seems like it
                        // has to go on the stack
                        // but since this is the end of the function either way, it
                        // might not have to
                        // hashBlankNodes(unnamed);
                        hui = -1;
                        unnamed = nextUnnamed;
                        nextUnnamed = new List<string>();
                        duplicates = new Dictionary<string, IList<string>>();
                        unique = new Dictionary<string, string>();
                        continue;
                    }

                    // name the duplicate hash bnods
                    // names duplicate hash bnodes
                    // enumerate duplicate hash groups in sorted order
                    hashes = new List<string>(duplicates.Keys);
                    hashes.SortInPlace();

                    // process each group
                    for (var pgi = 0;; pgi++)
                    {
                        if (pgi == hashes.Count)
                        {
                            // done, create JSON-LD array
                            // return createArray();
                            IList<string> normalized = new List<string>();

                            // Note: At this point all bnodes in the set of RDF
                            // quads have been
                            // assigned canonical names, which have been stored
                            // in the 'namer' object.
                            // Here each quad is updated by assigning each of
                            // its bnodes its new name
                            // via the 'namer' object
                            // update bnode names in each quad and serialize
                            for (var cai = 0; cai < quads.Count; ++cai)
                            {
                                var quad = quads[cai];
                                foreach (var attr in new[] {"subject", "object", "name"})
                                    if (quad.ContainsKey(attr))
                                    {
                                        var qa = (IDictionary<string, object>)quad[attr];
                                        if (qa != null && (string)qa["type"] == "blank node" && ((string)qa["value"]).IndexOf
                                                ("_:c14n") != 0)
                                            qa["value"] = namer.GetName((string)qa["value"]);
                                    }

                                normalized.Add(RdfDatasetUtils.ToNQuad(quad,
                                                                       quad.ContainsKey("name"
                                                                       ) && !(quad["name"] == null)
                                                                           ? (string)((IDictionary<string, object>)((IDictionary<string, object>)quad)
                                                                                         ["name"])["value"]
                                                                           : null));
                            }

                            // sort normalized output
                            normalized.SortInPlace();

                            // handle output format
                            if (options.format != null)
                                if ("application/nquads".Equals(options.format))
                                {
                                    var rval = string.Empty;
                                    foreach (var n in normalized) rval += n;
                                    return rval;
                                }
                                else
                                {
                                    throw new JsonLdError(JsonLdError.Error.UnknownFormat, options.format);
                                }

                            var rval_1 = string.Empty;
                            foreach (var n_1 in normalized) rval_1 += n_1;
                            return RdfDatasetUtils.ParseNQuads(rval_1);
                        }

                        // name each group member
                        var group = duplicates[hashes[pgi]];
                        IList<HashResult> results = new List<HashResult>();
                        for (var n_2 = 0;; n_2++)
                            if (n_2 == group.Count)
                            {
                                // name bnodes in hash order
                                results.SortInPlace(new _IComparer_145());
                                foreach (var r in results)

                                    // name all bnodes in path namer in
                                    // key-entry order
                                    // Note: key-order is preserved in
                                    // javascript
                                foreach (var key in r.pathNamer.Existing().GetKeys())
                                    namer.GetName(key);

                                // processGroup(i+1);
                                break;
                            }
                            else
                            {
                                // skip already-named bnodes
                                var bnode = group[n_2];
                                if (namer.IsNamed(bnode)) continue;

                                // hash bnode paths
                                var pathNamer = new UniqueNamer("_:b");
                                pathNamer.GetName(bnode);
                                var result = HashPaths(bnode, bnodes, namer, pathNamer);
                                results.Add(result);
                            }
                    }
                }

                // hash unnamed bnode
                var bnode_1 = unnamed[hui];
                var hash_1 = HashQuads(bnode_1, bnodes, namer);

                // store hash as unique or a duplicate
                if (duplicates.ContainsKey(hash_1))
                {
                    duplicates[hash_1].Add(bnode_1);
                    nextUnnamed.Add(bnode_1);
                }
                else
                {
                    if (unique.ContainsKey(hash_1))
                    {
                        IList<string> tmp = new List<string>();
                        tmp.Add(unique[hash_1]);
                        tmp.Add(bnode_1);
                        duplicates[hash_1] = tmp;
                        nextUnnamed.Add(unique[hash_1]);
                        nextUnnamed.Add(bnode_1);
                        Collections.Remove(unique, hash_1);
                    }
                    else
                    {
                        unique[hash_1] = bnode_1;
                    }
                }
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }

        /// <summary>
        ///     Produces a hash for the paths of adjacent bnodes for a bnode,
        ///     incorporating all information about its subgraph of bnodes.
        /// </summary>
        /// <remarks>
        ///     Produces a hash for the paths of adjacent bnodes for a bnode,
        ///     incorporating all information about its subgraph of bnodes. This method
        ///     will recursively pick adjacent bnode permutations that produce the
        ///     lexicographically-least 'path' serializations.
        /// </remarks>
        /// <param name="id">the ID of the bnode to hash paths for.</param>
        /// <param name="bnodes">the map of bnode quads.</param>
        /// <param name="namer">the canonical bnode namer.</param>
        /// <param name="pathNamer">the namer used to assign names to adjacent bnodes.</param>
        /// <param name="callback">(err, result) called once the operation completes.</param>
        private static HashResult HashPaths(string id,
                                            IDictionary<string, IDictionary<string, object>> bnodes,
                                            UniqueNamer namer,
                                            UniqueNamer pathNamer)
        {
#if !PORTABLE
            MessageDigest md = null;

            try
            {
                // create SHA-1 digest
                md = MessageDigest.GetInstance("SHA-1");
                var groups = new JObject();
                IList<string> groupHashes;
                var quads = (IList<RdfDataset.Quad>)bnodes[id]["quads"];
                for (var hpi = 0;; hpi++)
                {
                    if (hpi == quads.Count)
                    {
                        // done , hash groups
                        groupHashes = new List<string>(groups.GetKeys());
                        ((List<string>)groupHashes).Sort(StringComparer.CurrentCultureIgnoreCase);
                        for (var hgi = 0;; hgi++)
                        {
                            if (hgi == groupHashes.Count)
                            {
                                var res = new HashResult();
                                res.hash = EncodeHex(md.Digest());
                                res.pathNamer = pathNamer;
                                return res;
                            }

                            // digest group hash
                            var groupHash = groupHashes[hgi];
                            md.Update(JavaCompat.GetBytesForString(groupHash, "UTF-8"));

                            // choose a path and namer from the permutations
                            string chosenPath = null;
                            UniqueNamer chosenNamer = null;
                            var permutator = new Permutator((JArray)groups[groupHash]);
                            while (true)
                            {
                                var contPermutation = false;
                                var breakOut = false;
                                var permutation = permutator.Next();
                                var pathNamerCopy = pathNamer.Clone();

                                // build adjacent path
                                var path = string.Empty;
                                var recurse = new JArray();
                                foreach (string bnode in permutation)
                                {
                                    // use canonical name if available
                                    if (namer.IsNamed(bnode))
                                    {
                                        path += namer.GetName(bnode);
                                    }
                                    else
                                    {
                                        // recurse if bnode isn't named in the path
                                        // yet
                                        if (!pathNamerCopy.IsNamed(bnode)) recurse.Add(bnode);
                                        path += pathNamerCopy.GetName(bnode);
                                    }

                                    // skip permutation if path is already >= chosen
                                    // path
                                    if (chosenPath != null && path.Length >= chosenPath.Length && string.CompareOrdinal(path, chosenPath) > 0)
                                    {
                                        // return nextPermutation(true);
                                        if (permutator.HasNext())
                                        {
                                            contPermutation = true;
                                        }
                                        else
                                        {
                                            // digest chosen path and update namer
                                            md.Update(JavaCompat.GetBytesForString(chosenPath, "UTF-8"));
                                            pathNamer = chosenNamer;

                                            // hash the nextGroup
                                            breakOut = true;
                                        }

                                        break;
                                    }
                                }

                                // if we should do the next permutation
                                if (contPermutation) continue;

                                // if we should stop processing this group
                                if (breakOut) break;

                                // does the next recursion
                                for (var nrn = 0;; nrn++)
                                {
                                    if (nrn == recurse.Count)
                                    {
                                        // return nextPermutation(false);
                                        if (chosenPath == null || string.CompareOrdinal(path, chosenPath) < 0)
                                        {
                                            chosenPath = path;
                                            chosenNamer = pathNamerCopy;
                                        }

                                        if (!permutator.HasNext())
                                        {
                                            // digest chosen path and update namer
                                            md.Update(JavaCompat.GetBytesForString(chosenPath, "UTF-8"));
                                            pathNamer = chosenNamer;

                                            // hash the nextGroup
                                            breakOut = true;
                                        }

                                        break;
                                    }

                                    // do recursion
                                    var bnode_1 = (string)recurse[nrn];
                                    var result = HashPaths(bnode_1, bnodes, namer, pathNamerCopy);
                                    path += pathNamerCopy.GetName(bnode_1) + "<" + result.hash + ">";
                                    pathNamerCopy = result.pathNamer;

                                    // skip permutation if path is already >= chosen
                                    // path
                                    if (chosenPath != null && path.Length >= chosenPath.Length && string.CompareOrdinal(path, chosenPath) > 0)
                                    {
                                        // return nextPermutation(true);
                                        if (!permutator.HasNext())
                                        {
                                            // digest chosen path and update namer
                                            md.Update(JavaCompat.GetBytesForString(chosenPath, "UTF-8"));
                                            pathNamer = chosenNamer;

                                            // hash the nextGroup
                                            breakOut = true;
                                        }

                                        break;
                                    }
                                }

                                // do next recursion
                                // if we should stop processing this group
                                if (breakOut) break;
                            }
                        }
                    }

                    // get adjacent bnode
                    var quad = (IDictionary<string, object>)quads[hpi];
                    var bnode_2 = GetAdjacentBlankNodeName((IDictionary<string, object>)quad["subject"
                                                           ],
                                                           id);
                    string direction = null;
                    if (bnode_2 != null)
                    {
                        // normal property
                        direction = "p";
                    }
                    else
                    {
                        bnode_2 = GetAdjacentBlankNodeName((IDictionary<string, object>)quad["object"],
                                                           id
                        );
                        if (bnode_2 != null) direction = "r";
                    }

                    if (bnode_2 != null)
                    {
                        // get bnode name (try canonical, path, then hash)
                        string name;
                        if (namer.IsNamed(bnode_2))
                        {
                            name = namer.GetName(bnode_2);
                        }
                        else
                        {
                            if (pathNamer.IsNamed(bnode_2))
                                name = pathNamer.GetName(bnode_2);
                            else
                                name = HashQuads(bnode_2, bnodes, namer);
                        }

                        // hash direction, property, end bnode name/hash
                        using (var md1 = MessageDigest.GetInstance("SHA-1"))
                        {
                            // String toHash = direction + (String) ((Map<String,
                            // Object>) quad.get("predicate")).get("value") + name;
                            md1.Update(JavaCompat.GetBytesForString(direction, "UTF-8"));
                            md1.Update(JavaCompat.GetBytesForString((string)((IDictionary<string, object>)quad["predicate"])["value"], "UTF-8"));
                            md1.Update(JavaCompat.GetBytesForString(name, "UTF-8"));
                            var groupHash = EncodeHex(md1.Digest());
                            if (groups.ContainsKey(groupHash))
                            {
                                ((JArray)groups[groupHash]).Add(bnode_2);
                            }
                            else
                            {
                                var tmp = new JArray();
                                tmp.Add(bnode_2);
                                groups[groupHash] = tmp;
                            }
                        }
                    }
                }
            }
            catch
            {
                // TODO: i don't expect that SHA-1 is even NOT going to be
                // available?
                // look into this further
                throw;
            }
            finally
            {
                md?.Dispose();
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }

        /// <summary>Hashes all of the quads about a blank node.</summary>
        /// <remarks>Hashes all of the quads about a blank node.</remarks>
        /// <param name="id">the ID of the bnode to hash quads for.</param>
        /// <param name="bnodes">the mapping of bnodes to quads.</param>
        /// <param name="namer">the canonical bnode namer.</param>
        /// <returns>the new hash.</returns>
        private static string HashQuads(string id,
                                        IDictionary<string, IDictionary<string, object>> bnodes,
                                        UniqueNamer
                                            namer)
        {
            // return cached hash
            if (bnodes[id].ContainsKey("hash")) return (string)bnodes[id]["hash"];

            // serialize all of bnode's quads
            var quads = (IList<RdfDataset.Quad>)bnodes[id]["quads"];
            IList<string> nquads = new List<string>();
            for (var i = 0; i < quads.Count; ++i)
            {
                object name;
                nquads.Add(RdfDatasetUtils.ToNQuad(quads[i],
                                                   quads[i].TryGetValue("name", out name)
                                                       ? (string)((IDictionary<string, object>)name)["value"]
                                                       : null,
                                                   id));
            }

            // sort serialized quads
            nquads.SortInPlace(StringComparer.Ordinal);

            // return hashed quads
            var hash = Sha1hash(nquads);
            bnodes[id]["hash"] = hash;
            return hash;
        }

        /// <summary>A helper class to sha1 hash all the strings in a collection</summary>
        /// <param name="nquads"></param>
        /// <returns></returns>
        private static string Sha1hash(ICollection<string> nquads)
        {
#if !PORTABLE
            try
            {
                // create SHA-1 digest
                var md = MessageDigest.GetInstance("SHA-1");
                foreach (var nquad in nquads) md.Update(JavaCompat.GetBytesForString(nquad, "UTF-8"));
                return EncodeHex(md.Digest());
            }
            catch
            {
                throw;
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }

        // TODO: this is something to optimize
        private static string EncodeHex(byte[] data)
        {
            var rval = string.Empty;
            foreach (var b in data) rval += b.ToString("x2");
            return rval;
        }

        /// <summary>
        ///     A helper function that gets the blank node name from an RDF quad node
        ///     (subject or object).
        /// </summary>
        /// <remarks>
        ///     A helper function that gets the blank node name from an RDF quad node
        ///     (subject or object). If the node is a blank node and its value does not
        ///     match the given blank node ID, it will be returned.
        /// </remarks>
        /// <param name="node">the RDF quad node.</param>
        /// <param name="id">the ID of the blank node to look next to.</param>
        /// <returns>the adjacent blank node name or null if none was found.</returns>
        private static string GetAdjacentBlankNodeName(IDictionary<string, object> node, string id)
        {
            return (string)node["type"] == "blank node" && (!node.ContainsKey("value") || (string)node["value"] != id) ? (string)node["value"] : null;
        }

        private sealed class _IComparer_145 : IComparer<HashResult>
        {
            public int Compare(HashResult a, HashResult b)
            {
                var res = string.CompareOrdinal(a.hash, b.hash);
                return res;
            }
        }

        private class HashResult
        {
            internal string hash;

            internal UniqueNamer pathNamer;
        }

        private class Permutator
        {
            private readonly IDictionary<string, bool> left;
            private readonly JArray list;

            private bool done;

            public Permutator(JArray list)
            {
                this.list = (JArray)JsonLdUtils.Clone(list);
                this.list.SortInPlace();
                done = false;
                left = new Dictionary<string, bool>();
                foreach (string i in this.list) left[i] = true;
            }

            /// <summary>Returns true if there is another permutation.</summary>
            /// <remarks>Returns true if there is another permutation.</remarks>
            /// <returns>true if there is another permutation, false if not.</returns>
            public virtual bool HasNext()
            {
                return !done;
            }

            /// <summary>Gets the next permutation.</summary>
            /// <remarks>
            ///     Gets the next permutation. Call hasNext() to ensure there is another
            ///     one first.
            /// </remarks>
            /// <returns>the next permutation.</returns>
            public virtual JArray Next()
            {
                var rval = (JArray)JsonLdUtils.Clone(list);

                // Calculate the next permutation using Steinhaus-Johnson-Trotter
                // permutation algoritm
                // get largest mobile element k
                // (mobile: element is grater than the one it is looking at)
                string k = null;
                var pos = 0;
                var length = list.Count;
                for (var i = 0; i < length; ++i)
                {
                    var element = (string)list[i];
                    var left = this.left[element];
                    if ((k == null || string.CompareOrdinal(element, k) > 0) &&
                        (left && i > 0 && string.CompareOrdinal(element, (string)list[i - 1]) > 0 ||
                         !left && i < length - 1 && string.CompareOrdinal(element, (string)list[i + 1]) > 0))
                    {
                        k = element;
                        pos = i;
                    }
                }

                // no more permutations
                if (k == null)
                {
                    done = true;
                }
                else
                {
                    // swap k and the element it is looking at
                    var swap = left[k] ? pos - 1 : pos + 1;
                    list[pos] = list[swap];
                    list[swap] = k;

                    // reverse the direction of all element larger than k
                    for (var i_1 = 0; i_1 < length; i_1++)
                        if (string.CompareOrdinal((string)list[i_1], k) > 0)
                            left[(string)list[i_1]] = !left[(string)list[i_1]];
                }

                return rval;
            }
        }
    }
}