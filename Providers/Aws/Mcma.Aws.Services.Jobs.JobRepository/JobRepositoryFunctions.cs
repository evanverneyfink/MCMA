using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mcma.Aws.Lambda.ApiGatewayProxy;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

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
        public async Task<APIGatewayProxyResponse> Api(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
        {
            return await LambdaApiGatewayProxy.Handle<Mcma.Services.Jobs.JobRepository.JobRepository>(request, lambdaContext);
        }
    }
}
