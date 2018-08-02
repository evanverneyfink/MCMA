using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Mcma.Server.Environment;

namespace Mcma.Aws.Lambda.ApiGatewayProxy
{
    public class StageVariableProvider : IEnvironmentVariableProvider
    {
        /// <summary>
        /// Instantiates an <see cref="StageVariableProvider"/>
        /// </summary>
        /// <param name="request"></param>
        public StageVariableProvider(APIGatewayProxyRequest request)
        {
            StageVariables = request.StageVariables ?? new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Gets the stage variables
        /// </summary>
        private IDictionary<string, string> StageVariables { get; }

        /// <summary>
        /// Gets the name as ApiGatewayProxy
        /// </summary>
        public string Name => "ApiGatewayProxy";

        /// <summary>
        /// Gets the priority of the config value provider
        /// </summary>
        public int Priority => 100;

        /// <summary>
        /// Gets flag indicating if environment variables can be set with this provider
        /// </summary>
        public bool CanSet => true;

        /// <summary>
        /// Checks if the environment variable provider has a variable with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key) => StageVariables.ContainsKey(key);

        /// <summary>
        /// Gets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => StageVariables[key];

        /// <summary>
        /// Sets an environment variable's value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value) => StageVariables[key] = value;
    }
}