using System;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Newtonsoft.Json.Linq;

namespace Mcma.JsonLd
{
    public interface IJsonLdResourceHelper
    {
        /// <summary>
        /// Gets a resource from the JSON in the body of the request
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<Resource> GetResourceFromJson(JToken json, Type type);

        /// <summary>
        /// Renders a resource to JSON using the provided JSON LD context
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<JToken> GetJsonFromResource(Resource resource, JToken context);
    }
}
