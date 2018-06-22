using Mcma.Core;

namespace Mcma.Server.Environment
{
    public static class EnvironmentExtensions
    {
        /// <summary>
        /// Gets the root path of the url
        /// </summary>
        public static string RootPath(this IEnvironment env) => env.Get<string>(nameof(RootPath));

        /// <summary>
        /// Gets the base public url for the server
        /// </summary>
        public static string PublicUrl(this IEnvironment env) => env.Get<string>(nameof(PublicUrl));

        /// <summary>
        /// Gets the name of the table in which to store resources
        /// </summary>
        public static string TableName(this IEnvironment env) => env.Get<string>(nameof(TableName));

        /// <summary>
        /// Gets the service registry url
        /// </summary>
        public static string ServiceRegistryUrl(this IEnvironment env) => env.Get<string>(nameof(ServiceRegistryUrl));

        /// <summary>
        /// Checks if a resource descriptor is for a local resource
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public static bool IsLocalResource(this IEnvironment environment, ResourceDescriptor resourceDescriptor)
        {
            return (resourceDescriptor?.Url?.StartsWith(environment.PublicUrl())).GetValueOrDefault();
        }
    }
}