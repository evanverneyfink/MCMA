using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Mcma.Server.Api;

namespace Mcma.Aws.Lambda.ApiGatewayProxy
{
    public class ApiGatewayProxyLambdaRequest : IRequest
    {
        /// <summary>
        /// Instantiates an <see cref="ApiGatewayProxyLambdaRequest"/>
        /// </summary>
        /// <param name="request"></param>
        public ApiGatewayProxyLambdaRequest(APIGatewayProxyRequest request)
        {
            Request = request;
        }

        /// <summary>
        /// Gets the underlying <see cref="APIGatewayProxyRequest"/>
        /// </summary>
        private APIGatewayProxyRequest Request { get; }

        /// <summary>
        /// Gets the HTTP method from the underlying <see cref="APIGatewayProxyRequest"/>
        /// </summary>
        public string Method => Request.HttpMethod;

        /// <summary>
        /// Gets the path of the request from the underlying <see cref="APIGatewayProxyRequest"/>
        /// </summary>
        public string Path => Request.Path;

        /// <summary>
        /// Gets the dictionary of query string parameters from the underlying <see cref="APIGatewayProxyRequest"/>
        /// </summary>
        public IDictionary<string, string> QueryParameters => Request.QueryStringParameters;

        /// <summary>
        /// Reads the body of the reqeust as JSON from the underlying <see cref="APIGatewayProxyRequest"/>
        /// </summary>
        /// <returns></returns>
        public Task<string> ReadBodyAsText() => Task.FromResult(Request.Body);

        /// <summary>
        /// Gets the response to be sent back to the requester using a <see cref="ApiGatewayProxyLambdaResponse"/>
        /// </summary>
        /// <returns></returns>
        public IResponse Response { get; } = new ApiGatewayProxyLambdaResponse();
    }
}