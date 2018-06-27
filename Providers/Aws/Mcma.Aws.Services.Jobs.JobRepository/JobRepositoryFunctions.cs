using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Mcma.Aws.Lambda.ApiGatewayProxy;
using Mcma.Extensions.Files.S3;
using Mcma.Extensions.Repositories.DynamoDb;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Mcma.Aws.Services.Jobs.JobRepository
{
    public class JobRepositoryFunctions
    {
        /// <summary>
        /// FIMS handler for API Gateway proxy requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lambdaContext"></param>
        /// <returns></returns>
        public Task<APIGatewayProxyResponse> Api(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
        {
            return LambdaApiGatewayProxy.Handle<Mcma.Services.Jobs.JobRepository.JobRepository>(
                request,
                lambdaContext,
                builder =>
                    builder.Services
                           .AddDynamoDbMcmaRepository()
                           .AddS3FileStorage());
        }
    }
}
