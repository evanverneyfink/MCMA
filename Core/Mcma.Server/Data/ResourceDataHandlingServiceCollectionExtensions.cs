using Mcma.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Server.Data
{
    public static class ResourceDataHandlingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds resource data handling
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddMcmaResourceDataHandling(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                   .AddBasicJsonSerialization()
                   .AddScoped<IDocumentHelper, DocumentHelper>()
                   .AddScoped(typeof(IRepositoryResourceDataHandler), typeof(RepositoryResourceDataHandler))
                   .AddScoped(typeof(IHttpResourceDataHandler), typeof(HttpResourceDataHandler))
                   .AddScoped(typeof(IResourceDataHandler), typeof(ResourceDataHandler));
        }
    }
}