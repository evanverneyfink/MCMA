using System.Threading.Tasks;
using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace Mcma.JsonLd
{
    public class CachedDocumentLoader : DocumentLoader
    {
        /// <summary>
        /// Instantiates a <see cref="CachedDocumentLoader"/>
        /// </summary>
        /// <param name="manager"></param>
        public CachedDocumentLoader(IJsonLdContextManager manager)
        {
            Manager = manager;
        }

        /// <summary>
        /// Gets the underlying cache
        /// </summary>
        private IJsonLdContextManager Manager { get; }

        /// <summary>
        /// Gets a <see cref="RemoteDocument"/> by first checking the cahse
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public override async Task<RemoteDocument> LoadDocumentAsync(string url)
        {
            // clean the cache to ensure we don't use stale data
            Manager.Clean();

            // check the cache first
            var context = Manager.Get(url);

            // if we found it in the cache, use that; otherwise, use base to get over HTTP
            if (context != null)
                return new RemoteDocument(url, context);

            // load context over HTTP
            var doc = await base.LoadDocumentAsync(url);

            // cache the result
            Manager.Set(url, new Context((JObject)doc.Document));

            return doc;
        }
    }
}