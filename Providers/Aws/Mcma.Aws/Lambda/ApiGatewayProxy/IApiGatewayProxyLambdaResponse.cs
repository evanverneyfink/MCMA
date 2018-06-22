using Amazon.Lambda.APIGatewayEvents;
using Mcma.Server.Api;

namespace Mcma.Aws.Lambda.ApiGatewayProxy
{
    public interface IApiGatewayProxyLambdaResponse : IResponse
    {
        /// <summary>
        /// Gets the response as an <see cref="APIGatewayProxyResponse"/>
        /// </summary>
        /// <returns></returns>
        APIGatewayProxyResponse AsAwsApiGatewayProxyResponse();
    }
}