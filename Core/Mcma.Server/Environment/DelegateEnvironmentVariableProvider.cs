using System;

namespace Mcma.Server.Environment
{
    public class DelegateEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        /// <summary>
        /// Instantiates a <see cref="DelegateEnvironmentVariableProvider"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <param name="keyChecker"></param>
        public DelegateEnvironmentVariableProvider(string name,
                                                   Func<string, string> getter,
                                                   Action<string, string> setter = null,
                                                   Func<string, bool> keyChecker = null)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
            KeyChecker = keyChecker;
        }

        /// <summary>
        /// Gets the delegate used to get environment variable values
        /// </summary>
        private Func<string, string> Getter { get; }

        /// <summary>
        /// Gets the delegate used to set environment variable values
        /// </summary>
        private Action<string, string> Setter { get; }

        /// <summary>
        /// Gets the delegate used to check if a key is present in the variables
        /// </summary>
        private Func<string, bool> KeyChecker { get; }

        /// <summary>
        /// Gets the name of the environment variable provider
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets flag indicating if environment variables can be set with this provider
        /// </summary>
        public bool CanSet => Setter != null;

        /// <summary>
        /// Checks if the environment variable provider has a variable with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key) => KeyChecker?.Invoke(key) ?? Getter?.Invoke(key) != null;

        /// <summary>
        /// Gets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => Getter?.Invoke(key);

        /// <summary>
        /// Sets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value) => Setter?.Invoke(key, value);
    }
}