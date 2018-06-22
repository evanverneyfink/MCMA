using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Mcma.Server.Api;
using Newtonsoft.Json.Linq;

namespace Mcma.Aws.Lambda.ApiGatewayProxy
{
    public class ApiGatewayProxyLambdaResponse : IApiGatewayProxyLambdaResponse
    {
        /// <summary>
        /// Gets the underlying response
        /// </summary>
        public APIGatewayProxyResponse Response { get; } = new APIGatewayProxyResponse();

        /// <summary>
        /// Sets the status of the response
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public IResponse WithStatus(HttpStatusCode status)
        {
            Response.StatusCode = (int)status;
            return this;
        }

        /// <summary>
        /// Sets a header on the response
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IResponse WithHeader(string header, string value)
        {
            if (Response.Headers == null)
                Response.Headers = new Dictionary<string, string>();
            Response.Headers[header] = value;
            return this;
        }

        /// <summary>
        /// Sets the body on the response to plain text
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IResponse WithPlainTextBody(string message)
        {
            Response.Body = message;
            return WithHeader("Content-Type", "text/plain");
        }

        /// <summary>
        /// Sets the body on the response to JSON
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        public IResponse WithJsonBody(JToken jToken)
        {
            Response.Body = jToken.ToString();
            return WithHeader("Content-Type", "application/json");
        }

        /// <summary>
        /// Gets the response as an <see cref="APIGatewayProxyResponse"/>
        /// </summary>
        /// <returns></returns>
        public APIGatewayProxyResponse AsAwsApiGatewayProxyResponse()
        {
            return Response;
        }
    }
}