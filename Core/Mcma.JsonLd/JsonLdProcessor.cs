using System.Threading.Tasks;
using JsonLD.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Mcma.JsonLd
{
    public class JsonLdProcessor : IJsonLdProcessor
    {
        /// <summary>
        /// Instantiates a <see cref="JsonLdProcessor"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="documentLoader"></param>
        public JsonLdProcessor(IOptions<JsonLdOptions> options, IDocumentLoader documentLoader)
        {
            Options = options?.Value ?? new JsonLdOptions();

            if (Options.format == null)
                Options.format = "application/nquads";

            Options.documentLoader = documentLoader;
        }

        /// <summary>
        /// Gets the options
        /// </summary>
        private JsonLdOptions Options { get;  }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> Compact method
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<JObject> Compact(JToken doc, JToken context = null)
        {
            return JsonLD.Core.JsonLdProcessor.CompactAsync(doc, context ?? Contexts.Default.Url, Options);
        }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> ExpandAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<JArray> ExpandAsync(JToken doc, Context context)
        {
            return JsonLD.Core.JsonLdProcessor.ExpandAsync(doc, Options);
        }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> FlattenAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Task<JToken> FlattenAsync(JToken doc)
        {
            return JsonLD.Core.JsonLdProcessor.FlattenAsync(doc, Options);
        }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> FrameAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Task<JObject> FrameAsync(JToken doc, JToken frame)
        {
            return JsonLD.Core.JsonLdProcessor.FrameAsync(doc, frame, Options);
        }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> NormalizeAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Task<object> NormalizeAsync(JToken doc)
        {
            return JsonLD.Core.JsonLdProcessor.NormalizeAsync(doc, Options);
        }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> FromRdfAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Task<JToken> FromRdfAsync(JToken doc)
        {
            return JsonLD.Core.JsonLdProcessor.FromRdfAsync(doc, Options);
        }

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> ToRdfAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Task<object> ToRdfAsync(JToken doc)
        {
            return JsonLD.Core.JsonLdProcessor.ToRdfAsync(doc, Options);
        }
    }
}