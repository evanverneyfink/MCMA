using System.IO;
using System.Net;
using System.Text;
using Mcma.Server.Api;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Mcma.Azure.Http
{
    public class ActionResultResponse : IActionResultResponse
    {
        /// <summary>
        /// Gets the underlying response
        /// </summary>
        private HttpResponseResult Response { get; } = new HttpResponseResult();

        /// <summary>
        /// Sets the status of the response
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public IResponse WithStatus(HttpStatusCode status)
        {
            Response.StatusCode = status;
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
            if (Response.Headers.ContainsKey(header))
                Response.Headers.Remove(header);

            Response.Headers.Add(header, value);

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
            Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(message));
            return this;
        }

        /// <summary>
        /// Sets the body on the response to JSON
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        public IResponse WithJsonBody(JToken jToken)
        {
            Response.ContentType = "application/json";
            Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(jToken.ToString()));
            return this;
        }

        /// <summary>
        /// Gets the response as an <see cref="IActionResult"/>
        /// </summary>
        /// <returns></returns>
        public IActionResult AsActionResult() => Response;
    }
}