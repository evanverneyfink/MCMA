using System;
using Mcma.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mcma.Json
{
    public static class JsonSerializationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds basic JSON serialization
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureSettings"></param>
        /// <returns></returns>
        public static IServiceCollection AddBasicJsonSerialization(this IServiceCollection serviceCollection,
                                                                   Action<JsonSerializerSettings> configureSettings = null)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = {new TypeConverter()}
            };

            configureSettings?.Invoke(settings);

            serviceCollection.AddOptions().Configure<JsonResourceSerializationOptions>(opts => opts.JsonSerializerSettings = settings);

            return serviceCollection.AddSingleton<IResourceSerializer, JsonResourceSerializer>();
        }
    }
}