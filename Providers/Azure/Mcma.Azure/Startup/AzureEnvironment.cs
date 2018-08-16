using System;
using System.Collections.Generic;
using Mcma.Server.Environment;

namespace Mcma.Azure.Startup
{
    public static class AzureEnvironment
    {
        /// <summary>
        /// Gets the host name
        /// </summary>
        public static string HostName(this IEnvironment environment) => environment.Get<string>("WEBSITE_HOSTNAME");

        /// <summary>
        /// Gets the public url of the Azure Function
        /// </summary>
        private static string PublicUrl(IEnvironment environment)
        {
            var hostName = environment.HostName();
            var scheme = hostName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ? "http" : "https";

            return $"{scheme}://{hostName}/{environment.RootPath()}";
        }

        /// <summary>
        /// Gets a <see cref="InMemoryEnvironmentVariableProvider"/> that provides the public url
        /// </summary>
        /// <returns></returns>
        public static IEnvironmentVariableProvider GetPublicUrlProvider(IEnvironment e)
            => new InMemoryEnvironmentVariableProvider(new Dictionary<string, Func<string>>
            {
                {nameof(EnvironmentExtensions.PublicUrl), () => PublicUrl(e)}
            });
    }
}