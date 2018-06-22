using System.Collections.Generic;

namespace Mcma.Server.Environment
{
    public class EnvironmentOptions
    {
        /// <summary>
        /// Gets the mapping of alternate keys
        /// </summary>
        internal IDictionary<string, List<string>> AlternateKeys { get; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Gets the collection of environment variable providers
        /// </summary>
        internal List<IEnvironmentVariableProvider> VariableProviders { get; } =
            new List<IEnvironmentVariableProvider> {new SystemEnvironmentVariableProvider()};

        /// <summary>
        /// Allows mapping of alternate keys to get values from config
        /// </summary>
        /// <param name="key"></param>
        /// <param name="alternateKey"></param>
        public EnvironmentOptions AddAlternateKey(string key, string alternateKey)
        {
            (AlternateKeys.ContainsKey(key) ? AlternateKeys[key] : (AlternateKeys[key] = new List<string>())).Add(alternateKey);
            (AlternateKeys.ContainsKey(alternateKey) ? AlternateKeys[alternateKey] : (AlternateKeys[alternateKey] = new List<string>())).Add(key);
            return this;
        }

        /// <summary>
        /// Adds an environment variable provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public EnvironmentOptions AddProvider<T>() where T : IEnvironmentVariableProvider, new() => AddProvider(new T());

        /// <summary>
        /// Adds an environment variable provider
        /// </summary>
        /// <param name="provider"></param>
        public EnvironmentOptions AddProvider(IEnvironmentVariableProvider provider)
        {
            VariableProviders.Add(provider);
            return this;
        }
    }
}