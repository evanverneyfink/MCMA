using System;
using Mcma.Core;
using Mcma.Server.Files;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Extensions.Files.S3
{
    public static class S3ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DynamoDB as the repository service behind FIMS
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddS3FileStorage(this IServiceCollection services, Action<S3Options> configureOptions = null)
        {
            if (configureOptions != null)
                services.Configure(configureOptions);

            ResourceTypes.Add<AwsS3Locator>();
            return services.AddScoped<IFileStorage, S3FileStorage>();
        }
    }
}