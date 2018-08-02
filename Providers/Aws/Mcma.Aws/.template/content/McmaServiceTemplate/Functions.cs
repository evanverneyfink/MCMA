using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Mcma.Aws.Lambda.ApiGatewayProxy;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace McmaServiceTemplate
{
    public class Functions
    {
        public Task<APIGatewayProxyResponse> Api(APIGatewayProxyRequest input, ILambdaContext context)
        {
            return LambdaApiGatewayProxy.Handle<ResourceApiRegistration>(input, context);
        }
    }
}
