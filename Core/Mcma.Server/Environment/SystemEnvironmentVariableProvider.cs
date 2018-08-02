namespace Mcma.Server.Environment
{
    internal class SystemEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        /// <summary>
        /// Gets the name of the System environment variable provider
        /// </summary>
        public string Name => "System";

        /// <summary>
        /// Gets the priority of the variable provider
        /// </summary>
        public int Priority => int.MaxValue;

        /// <summary>
        /// Gets flag indicating if environment variables can be set with this provider
        /// </summary>
        public bool CanSet => true;

        /// <summary>
        /// Checks if the environment variable provider has a variable with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key) => System.Environment.GetEnvironmentVariables().Contains(key);

        /// <summary>
        /// Gets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => System.Environment.GetEnvironmentVariables().Contains(key)
                                             ? (string)System.Environment.GetEnvironmentVariables()[key]
                                             : string.Empty;

        /// <summary>
        /// Sets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value) => System.Environment.SetEnvironmentVariable(key, value);
    }
}