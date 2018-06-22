using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mcma.Services.Ame.MediaInfo
{
    public static class MediaInfoServiceCollectionExtensions
    {
        public static IServiceCollection AddMediaInfo<TAccessibleUrlProvider, TProcessLocator>(this IServiceCollection services)
            where TAccessibleUrlProvider : class, IMediaInfoAccessibleLocationProvider
            where TProcessLocator : class, IMediaInfoProcessLocator
        {
            services.TryAdd(ServiceDescriptor.Scoped<IProcessRunner, ProcessRunner>());
            services.TryAdd(ServiceDescriptor.Scoped<IMediaInfoOutputConverter, MediaInfoOutputConverter>());

            services.TryAdd(ServiceDescriptor.Scoped<IMediaInfoAccessibleLocationProvider, TAccessibleUrlProvider>());
            services.TryAdd(ServiceDescriptor.Scoped<IMediaInfoProcessLocator, TProcessLocator>());

            return services;
        }

        public static IServiceCollection AddLocalMediaInfo(this IServiceCollection services)
        {
            return services.AddMediaInfo<LocalMediaInfoAccessibleLocationProvider, LocalWindowsProcessLocator>();
        }
    }
}