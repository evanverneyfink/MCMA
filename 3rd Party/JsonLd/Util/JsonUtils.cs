using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonLD.Util
{
    /// <summary>A bunch of functions to make loading JSON easy</summary>
    /// <author>tristan</author>
    public class JsonUtils
    {
        /// <summary>An HTTP Accept header that prefers JSONLD.</summary>
        /// <remarks>An HTTP Accept header that prefers JSONLD.</remarks>
        protected internal const string AcceptHeader =
            "application/ld+json, application/json;q=0.9, application/javascript;q=0.5, text/javascript;q=0.5, text/plain;q=0.2, */*;q=0.1";

        //private static readonly ObjectMapper JsonMapper = new ObjectMapper();

        //private static readonly JsonFactory JsonFactory = new JsonFactory(JsonMapper);

        // private static volatile IHttpClient httpClient;

        /// <exception cref="Com.Fasterxml.Jackson.Core.JsonParseException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public static JToken FromString(string jsonString)
        {
            return FromReader(new StringReader(jsonString));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static JToken FromReader(TextReader r)
        {
            var serializer = new JsonSerializer();

            using (var reader = new JsonTextReader(r))
            {
                var result = (JToken)serializer.Deserialize(reader);
                return result;
            }
        }

        /// <exception cref="Com.Fasterxml.Jackson.Core.JsonGenerationException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public static void Write(TextWriter w, JToken jsonObject)
        {
            var serializer = new JsonSerializer();
            using (var writer = new JsonTextWriter(w))
            {
                serializer.Serialize(writer, jsonObject);
            }
        }

        /// <exception cref="Com.Fasterxml.Jackson.Core.JsonGenerationException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public static void WritePrettyPrint(TextWriter w, JToken jsonObject)
        {
            var serializer = new JsonSerializer();
            using (var writer = new JsonTextWriter(w))
            {
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, jsonObject);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static JToken FromInputStream(Stream content)
        {
            return FromInputStream(content, "UTF-8");
        }

        // no readers from
        // inputstreams w.o.
        // encoding!!
        /// <exception cref="System.IO.IOException"></exception>
        public static JToken FromInputStream(Stream content, string enc)
        {
            return FromReader(new StreamReader(content, Encoding.GetEncoding(enc)));
        }

        public static string ToPrettyString(JToken obj)
        {
            var sw = new StringWriter();
            try
            {
                WritePrettyPrint(sw, obj);
            }
            catch
            {
                // TODO Is this really possible with stringwriter?
                // I think it's only there because of the interface
                // however, if so... well, we have to do something!
                // it seems weird for toString to throw an IOException
                throw;
            }

            return sw.ToString();
        }

        public static string ToString(JToken obj)
        {
            // throws
            // JsonGenerationException,
            // JsonMappingException {
            var sw = new StringWriter();
            try
            {
                Write(sw, obj);
            }
            catch
            {
                // TODO Is this really possible with stringwriter?
                // I think it's only there because of the interface
                // however, if so... well, we have to do something!
                // it seems weird for toString to throw an IOException
                throw;
            }

            return sw.ToString();
        }

        /// <summary>
        ///     Returns a Map, List, or String containing the contents of the JSON
        ///     resource resolved from the Url.
        /// </summary>
        /// <remarks>
        ///     Returns a Map, List, or String containing the contents of the JSON
        ///     resource resolved from the Url.
        /// </remarks>
        /// <param name="url">The Url to resolve</param>
        /// <returns>
        ///     The Map, List, or String that represent the JSON resource
        ///     resolved from the Url
        /// </returns>
        /// <exception cref="Com.Fasterxml.Jackson.Core.JsonParseException">
        ///     If the JSON was not valid.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///     If there was an error resolving the resource.
        /// </exception>
        public static JToken FromURL(Uri url)
        {
#if !PORTABLE && !IS_CORECLR
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Accept = AcceptHeader;
            var resp = req.GetResponse();
            var stream = resp.GetResponseStream();
            return FromInputStream(stream);
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}