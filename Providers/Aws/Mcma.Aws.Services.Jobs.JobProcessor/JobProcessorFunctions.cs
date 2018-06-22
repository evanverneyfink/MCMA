using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mcma.Aws.Lambda.ApiGatewayProxy;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Mcma.Aws.Services.Jobs.JobProcessor
{
    public static class JobProcessorFunctions
    {
        public static Task<APIGatewayProxyResponse> Api(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return LambdaApiGatewayProxy.Handle<Mcma.Services.Jobs.JobProcessor.JobProcessor>(request, context);
        }
    }
}
