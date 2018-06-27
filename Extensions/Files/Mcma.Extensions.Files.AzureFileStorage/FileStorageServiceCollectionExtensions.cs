using System;
using Mcma.Core;
using Mcma.Server.Files;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Extensions.Files.AzureFileStorage
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureFileStorage(this IServiceCollection services, Action<FileStorageOptions> configureOptions = null)
        {
            if (configureOptions != null)
                services.Configure(configureOptions);

            ResourceTypes.Add<AzureFileStorageLocator>();
            return services.AddScoped<IFileStorage, AzureFileStorage>();
        }
    }
}