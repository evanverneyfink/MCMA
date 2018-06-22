using Mcma.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Server.Files
{
    public static class LocalFileStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds local file storage
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddLocalFileStorage(this IServiceCollection serviceCollection)
        {
            ResourceTypes.Add<LocalLocator>();

            return serviceCollection.AddSingleton<IFileStorage, LocalFileStorage>();
        }
    }
}