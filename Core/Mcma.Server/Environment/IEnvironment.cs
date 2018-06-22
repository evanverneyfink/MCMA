namespace Mcma.Server.Environment
{
    public interface IEnvironment
    {
        /// <summary>
        /// Gets the value of an environment variable with the given key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// Sets the value of an environment variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="variableProviderName"></param>
        void Set<T>(string key, T value, string variableProviderName = null);
    }
}