using System.Net;
using System.Text;
using Mcma.Server.Api;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Mcma.WebApi
{
    public class WebApiResponse : IResponse
    {
        /// <summary>
        /// Instantiates a <see cref="WebApiResponse"/>
        /// </summary>
        /// <param name="response"></param>
        public WebApiResponse(HttpResponse response)
        {
            Response = response;
        }

        /// <summary>
        /// Gets the underlying response
        /// </summary>
        private HttpResponse Response { get; }

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
            Response.ContentType = "text/plain";
            return WriteBody(message);
        }

        /// <summary>
        /// Sets the body on the response to JSON
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        public IResponse WithJsonBody(JToken jToken)
        {
            Response.ContentType = "application/json";
            return WriteBody(jToken.ToString());
        }

        /// <summary>
        /// Writes text to the body of the response
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IResponse WriteBody(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            Response.Body.Write(bytes, 0, bytes.Length);
            return this;
        }
    }
}