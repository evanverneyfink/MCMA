using System;
using System.Collections.Generic;

namespace Mcma.Server.Environment
{
    public class InMemoryEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        /// <summary>
        /// Instantiates a <see cref="InMemoryEnvironmentVariableProvider"/>
        /// </summary>
        /// <param name="valueProviders"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        public InMemoryEnvironmentVariableProvider(IDictionary<string, Func<string>> valueProviders,
                                                   string name = null,
                                                   int priority = int.MaxValue - 1)
        {
            ValueProviders = valueProviders;
            Name = name;
            Priority = priority;
        }

        /// <summary>
        /// Gets the collection of value providers
        /// </summary>
        private IDictionary<string, Func<string>> ValueProviders { get; }

        /// <summary>
        /// Gets the name of environment variable provider
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the priority of the variable provider
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Gets flag indicating if environment variables can be set with this provider
        /// </summary>
        public bool CanSet => true;

        /// <summary>
        /// Checks if the environment variable provider has a variable with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key) => ValueProviders.ContainsKey(key);

        /// <summary>
        /// Gets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => ValueProviders.ContainsKey(key) ? ValueProviders[key]() : null;

        /// <summary>
        /// Sets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value) => ValueProviders[key] = () => value;
    }
}