using Mcma.Server.Environment;
using Microsoft.Extensions.Configuration;

namespace Mcma.Azure
{
    public class ConfigValueProvider : IEnvironmentVariableProvider
    {
        /// <summary>
        /// Instantiates a <see cref="ConfigValueProvider"/>
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigValueProvider(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        /// <summary>
        /// Gets the configuration
        /// </summary>
        private IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the name of environment variable provider
        /// </summary>
        public string Name => "WebApiConfig";

        /// <summary>
        /// Gets flag indicating if environment variables can be set with this provider
        /// </summary>
        public bool CanSet => true;

        /// <summary>
        /// Checks if the environment variable provider has a variable with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key) => Configuration[key] != null;

        /// <summary>
        /// Gets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => Configuration[key];

        /// <summary>
        /// Sets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value) => Configuration[key] = value;
    }
}