using System;
using System.Collections.Generic;
using Mcma.Server.Environment;

namespace Mcma.Azure.Startup
{
    public static class AzureFunctionPublicUrl
    {
        /// <summary>
        /// The environment variable name for the website host name for an Azure Function
        /// </summary>
        public const string HostNameSetting = "WEBSITE_HOSTNAME";

        /// <summary>
        /// Gets the host name of the Azure Function
        /// </summary>
        public static string HostName => System.Environment.GetEnvironmentVariable(HostNameSetting);

        /// <summary>
        /// Gets the scheme (needs to be http for localhost)
        /// </summary>
        public static string Scheme => HostName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ? "http" : "https";

        /// <summary>
        /// Gets the public url of the Azure Function
        /// </summary>
        public static string PublicUrl => $"{Scheme}://{HostName}";

        /// <summary>
        /// Gets a <see cref="InMemoryEnvironmentVariableProvider"/> that provides the public url
        /// </summary>
        /// <returns></returns>
        public static InMemoryEnvironmentVariableProvider GetEnvironmentVariableProvider()
            => new InMemoryEnvironmentVariableProvider(new Dictionary<string, Func<string>>
            {
                {nameof(EnvironmentExtensions.PublicUrl), () => PublicUrl}
            });
    }
}