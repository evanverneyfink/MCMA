using System;
using System.Collections.Generic;
using System.Linq;
using Mcma.Core;

namespace Mcma.Server.Environment
{
    public class Environment : IEnvironment
    {
        /// <summary>
        /// Instantiates an <see cref="Environment"/>
        /// </summary>
        /// <param name="options"></param>
        public Environment(EnvironmentOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Gets environment options
        /// </summary>
        private EnvironmentOptions Options { get; }

        /// <summary>
        /// Gets the collection of environment variable providers
        /// </summary>
        private List<IEnvironmentVariableProvider> VariableProviders => Options.VariableProviders;

        /// <summary>
        /// Gets the value of an environment variable with the given key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            foreach (var k in new[] {key}.Concat(Options.AlternateKeys.ContainsKey(key) ? Options.AlternateKeys[key] : new List<string>()))
            {
                var variableProvider = VariableProviders.FirstOrDefault(vp => vp.HasKey(k));
                if (variableProvider != null && variableProvider.Get(k).TryParse<T>(out var value))
                    return value;
            }

            return default(T);
        }

        /// <summary>
        /// Sets the value of an environment variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="variableProviderName"></param>
        public void Set<T>(string key, T value, string variableProviderName = null)
        {
            IEnvironmentVariableProvider provider;
            if (variableProviderName != null)
            {
                provider = VariableProviders.FirstOrDefault(vp => vp.Name.Equals(variableProviderName, StringComparison.OrdinalIgnoreCase));
                if (provider == null)
                    throw new Exception($"Invalid variable provider name '{variableProviderName}'.");
                if (!provider.CanSet)
                    throw new Exception($"The provider '{variableProviderName}' does not support setting of variable values.");
            }
            else
            {
                provider = VariableProviders.FirstOrDefault(vp => vp.CanSet);
                if (provider == null)
                    throw new Exception("None of the registered environment variable providers support setting of variable values.");
            }

            provider.Set(key, value.ToString());
        }
    }
}