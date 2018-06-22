using System.Threading.Tasks;
using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace Mcma.JsonLd
{
    public interface IJsonLdProcessor
    {
        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> Compact method
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<JObject> Compact(JToken doc, JToken context = null);

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> ExpandAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<JArray> ExpandAsync(JToken doc, Context context);

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> FlattenAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task<JToken> FlattenAsync(JToken doc);

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> FrameAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        Task<JObject> FrameAsync(JToken doc, JToken frame);

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> NormalizeAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task<object> NormalizeAsync(JToken doc);

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> FromRdfAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task<JToken> FromRdfAsync(JToken doc);

        /// <summary>
        /// Wrapper around <see cref="JsonLD.Core.JsonLdProcessor"/> ToRdfAsync method
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task<object> ToRdfAsync(JToken doc);
    }
}