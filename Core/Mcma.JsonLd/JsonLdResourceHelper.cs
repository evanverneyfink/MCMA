using System;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Newtonsoft.Json.Linq;

namespace Mcma.JsonLd
{
    public class JsonLdResourceHelper : IJsonLdResourceHelper
    {
        public JsonLdResourceHelper(IJsonLdContextManager jsonLdContextManager, IJsonLdProcessor jsonLdProcessor)
        {
            JsonLdContextManager = jsonLdContextManager;
            JsonLdProcessor = jsonLdProcessor;
        }

        /// <summary>
        /// Gets the JSON LD context manager
        /// </summary>
        private IJsonLdContextManager JsonLdContextManager { get; }

        /// <summary>
        /// Gets the JSON LD processor
        /// </summary>
        private IJsonLdProcessor JsonLdProcessor { get; }

        /// <summary>
        /// Gets a resource from the JSON in the body of the request
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Resource> GetResourceFromJson(JToken json, Type type)
        {
            Console.WriteLine("Compacting JSON: {0}", json);

            // if there's not context on the object, use the default
            if (json.Type == JTokenType.Object && json["@context"] == null)
                json["@context"] = JsonLdContextManager.DefaultUrl;

            // process the request JSON
            json = await JsonLdProcessor.Compact(json, JsonLdContextManager.DefaultUrl);

            Console.WriteLine("Compacted JSON: {0}", json);

            return null;

            // convert the JSON to a resource
            //return Resource.FromToken(json, type);
        }

        /// <summary>
        /// Renders a resource to JSON using the provided JSON LD context
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<JToken> GetJsonFromResource(Resource resource, JToken context)
        {
            if (resource == null)
                return null;

            if (context == null || context.Type == JTokenType.String && string.IsNullOrWhiteSpace(context.Value<string>()))
                context = JsonLdContextManager.DefaultUrl;

            return null;

            // process JSON with context and return
            //return await JsonLdProcessor.Compact(resource, context);
        }
    }
}