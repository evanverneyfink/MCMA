using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Server.Api
{
    public static class McmaServerApiServiceCollectionExtensions
    {
        public static IServiceCollection AddMcmaServerDefaultApi(this IServiceCollection serviceCollection)
        {
            return
                serviceCollection.AddScoped<IUrlSegmentResourceMapper, DefaultUrlSegmentResourceMapper>()
                                 .AddScoped<IResourceDescriptorHelper, DefaultResourceDescriptorHelper>()
                                 .AddScoped<IRequestHandler, DefaultRequestHandler>();
        }
    }
}
