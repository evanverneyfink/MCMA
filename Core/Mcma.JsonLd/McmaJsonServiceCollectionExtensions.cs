using JsonLD.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.JsonLd
{
    public static class McmaJsonServiceCollectionExtensions
    {
        public static IServiceCollection AddMcmaCore(this IServiceCollection serviceCollection)
        {
            return
                serviceCollection.AddOptions()
                                 .AddSingleton<IDocumentLoader, CachedDocumentLoader>()
                                 .AddSingleton<IJsonLdContextManager, JsonLdContextManager>()
                                 .Configure<JsonLdContextManagerOptions>(opts => { opts.DefaultContextUrl = Contexts.Default.Url; })
                                 .AddSingleton<IJsonLdProcessor, JsonLdProcessor>()
                                 .AddSingleton<IJsonLdResourceHelper, JsonLdResourceHelper>();
        }
    }
}
