using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Mcma.Aws.Lambda.ApiGatewayProxy;
using Mcma.Extensions.Files.S3;
using Mcma.Extensions.Repositories.DynamoDb;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Mcma.Aws.Services.Jobs.JobProcessor
{
    public static class JobProcessorFunctions
    {
        public static Task<APIGatewayProxyResponse> Api(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return LambdaApiGatewayProxy.Handle<Mcma.Services.Jobs.JobProcessor.JobProcessor>(
                request,
                context,
                builder =>
                    builder.Services
                           .AddDynamoDbMcmaRepository()
                           .AddS3FileStorage());
        }
    }
}
