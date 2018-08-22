using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Server.AuthorizedUrls
{
    public static class AuthorizedUrlBuilderServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizedUrlBuilding(this IServiceCollection services)
            => services.AddSingleton<IAuthorizedUrlBuilder, AuthorizedUrlBuilder>();

        public static IServiceCollection AddProviderAuthorizedUrlBuilder<T>(this IServiceCollection services)
            where T : class, IProviderSpecificAuthorizedUrlBuilder
            => services.AddSingleton<IProviderSpecificAuthorizedUrlBuilder, T>();
    }
}