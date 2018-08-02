namespace Mcma.Server.Environment
{
    public interface IEnvironmentVariableProvider
    {
        /// <summary>
        /// Gets the name of environment variable provider
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the priority of the variable provider
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets flag indicating if environment variables can be set with this provider
        /// </summary>
        bool CanSet { get; }

        /// <summary>
        /// Checks if the environment variable provider has a variable with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasKey(string key);

        /// <summary>
        /// Gets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string Get(string key);

        /// <summary>
        /// Sets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, string value);
    }
}