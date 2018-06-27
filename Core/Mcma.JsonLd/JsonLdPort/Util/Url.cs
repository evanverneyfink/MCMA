using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JsonLD.Util
{
    public class Url
    {
        private static readonly Regex parser =
            new Regex(
                "^(?:([^:\\/?#]+):)?(?:\\/\\/((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\\/?#]*)(?::(\\d*))?))?((((?:[^?#\\/]*\\/)*)([^?#]*))(?:\\?([^#]*))?(?:#(.*))?)");

        public string auth = string.Empty;

        public string authority;

        public string directory = string.Empty;

        public string file = string.Empty;

        public string hash = string.Empty;

        public string host = string.Empty;

        public string hostname = string.Empty;
        public string href = string.Empty;

        public string normalizedPath;

        public string password = string.Empty;

        public string path = string.Empty;

        public string pathname;

        public string port = string.Empty;

        public string protocol = string.Empty;

        public string query = string.Empty;

        public string relative = string.Empty;

        public string user = string.Empty;

        // things not populated by the regex (NOTE: i don't think it matters if
        // these are null or "" to start with)
        public static Url Parse(string url)
        {
            var rval = new Url();
            rval.href = url;
            var matches = parser.Matches(url);
            if (matches.Count > 0)
            {
                var matcher = matches[0];
                if (matcher.Groups[1] != null) rval.protocol = matcher.Groups[1].Value;
                if (matcher.Groups[2].Value != null) rval.host = matcher.Groups[2].Value;
                if (matcher.Groups[3].Value != null) rval.auth = matcher.Groups[3].Value;
                if (matcher.Groups[4].Value != null) rval.user = matcher.Groups[4].Value;
                if (matcher.Groups[5].Value != null) rval.password = matcher.Groups[5].Value;
                if (matcher.Groups[6].Value != null) rval.hostname = matcher.Groups[6].Value;
                if (matcher.Groups[7].Value != null) rval.port = matcher.Groups[7].Value;
                if (matcher.Groups[8].Value != null) rval.relative = matcher.Groups[8].Value;
                if (matcher.Groups[9].Value != null) rval.path = matcher.Groups[9].Value;
                if (matcher.Groups[10].Value != null) rval.directory = matcher.Groups[10].Value;
                if (matcher.Groups[11].Value != null) rval.file = matcher.Groups[11].Value;
                if (matcher.Groups[12].Value != null) rval.query = matcher.Groups[12].Value;
                if (matcher.Groups[13].Value != null) rval.hash = matcher.Groups[13].Value;

                // normalize to node.js API
                if (!string.Empty.Equals(rval.host) && string.Empty.Equals(rval.path)) rval.path = "/";
                rval.pathname = rval.path;
                ParseAuthority(rval);
                rval.normalizedPath = RemoveDotSegments(rval.pathname, !string.Empty.Equals(rval.authority));
                if (!string.Empty.Equals(rval.query)) rval.path += "?" + rval.query;
                if (!string.Empty.Equals(rval.protocol)) rval.protocol += ":";
                if (!string.Empty.Equals(rval.hash)) rval.hash = "#" + rval.hash;
                return rval;
            }

            return rval;
        }

        /// <summary>Removes dot segments from a Url path.</summary>
        /// <remarks>Removes dot segments from a Url path.</remarks>
        /// <param name="path">the path to remove dot segments from.</param>
        /// <param name="hasAuthority">true if the Url has an authority, false if not.</param>
        public static string RemoveDotSegments(string path, bool hasAuthority)
        {
            var rval = string.Empty;
            if (path.IndexOf("/") == 0) rval = "/";

            // RFC 3986 5.2.4 (reworked)
            IList<string> input = new List<string>(path.Split("/"
                                                   ).ToList());
            if (path.EndsWith("/")) input.Add(string.Empty);
            IList<string> output = new List<string>();
            for (var i = 0; i < input.Count; i++)
            {
                if (".".Equals(input[i]) || string.Empty.Equals(input[i]) && input.Count - i > 1) continue;
                if ("..".Equals(input[i]))
                {
                    // input.remove(0);
                    if (hasAuthority || output.Count > 0 && !"..".Equals(output[output.Count - 1]))
                    {
                        // [].pop() doesn't fail, to replicate this we need to check
                        // that there is something to remove
                        if (output.Count > 0) output.RemoveAt(output.Count - 1);
                    }
                    else
                    {
                        output.Add("..");
                    }

                    continue;
                }

                output.Add(input[i]);
            }

            // input.remove(0);
            if (output.Count > 0)
            {
                rval += output[0];
                for (var i_1 = 1; i_1 < output.Count; i_1++) rval += "/" + output[i_1];
            }

            return rval;
        }

        public static string RemoveBase(JToken baseobj, string iri)
        {
            if (baseobj.IsNull()) return iri;
            Url @base;
            if (baseobj.Type == JTokenType.String)
                @base = Parse((string)baseobj);
            else
                throw new Exception("Arrgggghhh!");

            // establish base root
            var root = string.Empty;
            if (!string.Empty.Equals(@base.href))
            {
                root += @base.protocol + "//" + @base.authority;
            }
            else
            {
                // support network-path reference with empty base
                if (iri.IndexOf("//") != 0) root += "//";
            }

            // IRI not relative to base
            if (iri.IndexOf(root) != 0) return iri;

            // remove root from IRI and parse remainder
            var rel = Parse(JavaCompat.Substring(iri, root.Length));

            // remove path segments that match
            IList<string> baseSegments = new List<string>(@base
                                                          .normalizedPath.Split("/").ToList());
            baseSegments = baseSegments.Where(seg => seg != "").ToList();
            if (@base.normalizedPath.EndsWith("/")) baseSegments.Add(string.Empty);
            IList<string> iriSegments = new List<string>(rel.normalizedPath
                                                            .Split("/").ToList());
            iriSegments = iriSegments.Where(seg => seg != "").ToList();
            if (rel.normalizedPath.EndsWith("/")) iriSegments.Add(string.Empty);
            while (baseSegments.Count > 0 && iriSegments.Count > 0)
            {
                if (!baseSegments[0].Equals(iriSegments[0])) break;
                if (baseSegments.Count > 0) baseSegments.RemoveAt(0);
                if (iriSegments.Count > 0) iriSegments.RemoveAt(0);
            }

            // use '../' for each non-matching base segment
            var rval = string.Empty;
            if (baseSegments.Count > 0)
            {
                // don't count the last segment if it isn't a path (doesn't end in
                // '/')
                // don't count empty first segment, it means base began with '/'
                if (!@base.normalizedPath.EndsWith("/") || string.Empty.Equals(baseSegments[0])) baseSegments.RemoveAt(baseSegments.Count - 1);
                for (var i = 0; i < baseSegments.Count; ++i) rval += "../";
            }

            // prepend remaining segments
            if (iriSegments.Count > 0) rval += iriSegments[0];
            for (var i_1 = 1; i_1 < iriSegments.Count; i_1++) rval += "/" + iriSegments[i_1];

            // add query and hash
            if (!string.Empty.Equals(rel.query)) rval += "?" + rel.query;
            if (!string.Empty.Equals(rel.hash)) rval += rel.hash;
            if (string.Empty.Equals(rval)) rval = "./";
            return rval;
        }

        public static string Resolve(string baseUri, string pathToResolve)
        {
            // TODO: some input will need to be normalized to perform the expected
            // result with java
            // TODO: we can do this without using java URI!
            if (baseUri == null) return pathToResolve;
            if (pathToResolve == null || string.Empty.Equals(pathToResolve.Trim())) return baseUri;
            try
            {
                var uri = new Uri(baseUri);

                // query string parsing
                if (pathToResolve.StartsWith("?"))
                {
                    // drop fragment from uri if it has one
                    if (uri.Fragment != null) uri = new Uri(uri.Scheme + "://" + uri.Authority + uri.AbsolutePath);

                    // add query to the end manually (as URI.resolve does it wrong)
                    return uri + pathToResolve;
                }

                uri = new Uri(uri, pathToResolve);

                // java doesn't discard unnecessary dot segments
                var path = uri.AbsolutePath;
                if (path != null) path = RemoveDotSegments(uri.AbsolutePath, true);

                // TODO(sblom): This line is wrong, but works.
                return new Uri(uri.Scheme + "://" + uri.Authority + path + uri.Query + uri.Fragment).ToString
                    ();
            }
            catch
            {
                return pathToResolve;
            }
        }

        /// <summary>Parses the authority for the pre-parsed given Url.</summary>
        /// <remarks>Parses the authority for the pre-parsed given Url.</remarks>
        /// <param name="parsed">the pre-parsed Url.</param>
        private static void ParseAuthority(Url parsed)
        {
            // parse authority for unparsed relative network-path reference
            if (parsed.href.IndexOf(":") == -1 && parsed.href.IndexOf("//") == 0 && string.Empty
                                                                                          .Equals(parsed.host))
            {
                // must parse authority from pathname
                parsed.pathname = JavaCompat.Substring(parsed.pathname, 2);
                var idx = parsed.pathname.IndexOf("/");
                if (idx == -1)
                {
                    parsed.authority = parsed.pathname;
                    parsed.pathname = string.Empty;
                }
                else
                {
                    parsed.authority = JavaCompat.Substring(parsed.pathname, 0, idx);
                    parsed.pathname = JavaCompat.Substring(parsed.pathname, idx);
                }
            }
            else
            {
                // construct authority
                parsed.authority = parsed.host;
                if (!string.Empty.Equals(parsed.auth)) parsed.authority = parsed.auth + "@" + parsed.authority;
            }
        }
    }
}