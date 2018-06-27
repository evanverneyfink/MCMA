using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Mcma.Aws.Lambda.ApiGatewayProxy;
using Mcma.Extensions.Files.S3;
using Mcma.Extensions.Repositories.DynamoDb;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

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
            return LambdaApiGatewayProxy.Handle<Mcma.Services.ServiceRegistry.ServiceRegistry>(
                input,
                context,
                builder =>
                    builder.Services
                           .AddDynamoDbMcmaRepository()
                           .AddS3FileStorage());
        }
    }
}
