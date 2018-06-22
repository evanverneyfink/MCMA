using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mcma.Aws.Lambda.ApiGatewayProxy;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Mcma.Aws.Services.ServiceRegistry
{
    public class ServiceRegistryFunctions
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<APIGatewayProxyResponse> Api(APIGatewayProxyRequest input, ILambdaContext context)
        {
            return LambdaApiGatewayProxy.Handle<Mcma.Services.ServiceRegistry.ServiceRegistry>(input, context);
        }
    }
}
