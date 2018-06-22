using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonLD.Util;

namespace JsonLD.Core
{
    public class DocumentLoader : IDocumentLoader
    {
        /// <summary>
        /// Instantiates a <see cref="DocumentLoader"/>
        /// </summary>
        public DocumentLoader()
        {
            HttpClient = new HttpClient();

            // add accept headers
            foreach (var acceptHeaderVal in Accept.Select(MediaTypeWithQualityHeaderValue.Parse))
                HttpClient.DefaultRequestHeaders.Accept.Add(acceptHeaderVal);
        }

        /// <summary>An HTTP Accept header that prefers JSONLD.</summary>
        public static readonly string[] Accept =
        {
            "application/ld+json",
            "application/json;q=0.9",
            "application/javascript;q=0.5",
            "text/javascript;q=0.5",
            "text/plain;q=0.2",
            "*/*;q=0.1"
        };

        /// <summary>
        /// Gets the http client used to load documents
        /// </summary>
        private HttpClient HttpClient { get; }

        /// <summary>
        /// Loads a contenxt document
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public virtual async Task<RemoteDocument> LoadDocumentAsync(string url)
        {
            var doc = new RemoteDocument(url, null);
            try
            {
                var resp = await HttpClient.GetAsync(url);

                // ensure we got some form of JSON as a response
                if (!resp.Content.Headers.ContentType.MediaType.Contains("json"))
                    throw new JsonLdError(JsonLdError.Error.LoadingDocumentFailed, url);

                // check if the response contains JSON-LD
                var isJsonld = resp.Content.Headers.ContentType.MediaType == "application/ld+json";

                // get link headers from response
                var linkHeaders =
                    resp.Content.Headers.FirstOrDefault(kvp => kvp.Key == "Links").Value?
                        .SelectMany(h1 => h1.Split(",".ToCharArray()).Select(h2 => h2.Trim()))
                        .ToArray();

                if (!isJsonld && linkHeaders != null)
                {
                    // get headers for linked contexts (must only be 1)
                    var linkedContexts =
                        linkHeaders.Where(v => v.EndsWith("rel=\"http://www.w3.org/ns/json-ld#context\"", StringComparison.OrdinalIgnoreCase))
                                   .ToList();
                    if (linkedContexts.Count > 1)
                        throw new JsonLdError(JsonLdError.Error.MultipleContextLinkHeaders);

                    // get the url for the linked context from the response headers
                    var header = linkedContexts.First();
                    var linkedContextUrl = header.Substring(1, header.IndexOf(">", StringComparison.Ordinal) - 1);

                    // load the remote doc
                    var remoteContext = await LoadDocumentAsync(Url.Resolve(url, linkedContextUrl));

                    // set the context and its url
                    doc.ContextUrl = remoteContext.DocumentUrl;
                    doc.Context = remoteContext.Document;
                }

                doc.DocumentUrl = url;
                doc.Document = JsonUtils.FromInputStream(await resp.Content.ReadAsStreamAsync());
            }
            catch (JsonLdError)
            {
                throw;
            }
            catch (Exception)
            {
                throw new JsonLdError(JsonLdError.Error.LoadingDocumentFailed, url);
            }

            return doc;
        }
    }
}