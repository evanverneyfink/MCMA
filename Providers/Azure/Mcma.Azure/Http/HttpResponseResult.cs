using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mcma.Azure.Http
{
    public class HttpResponseResult : IActionResult
    {
        /// <summary>
        /// Gets or sets the HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets the collection of headers
        /// </summary>
        public IHeaderDictionary Headers { get; } = new HeaderDictionary();

        /// <summary>
        /// Gets or sets the content type of the response
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the body of the response
        /// </summary>
        public Stream Body { get; set; }

        /// <summary>
        /// Executes the action by setting the HTTP response
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)StatusCode;

            foreach (var header in Headers)
                context.HttpContext.Response.Headers[header.Key] = header.Value;

            context.HttpContext.Response.ContentType = ContentType;

            return Body.CopyToAsync(context.HttpContext.Response.Body);
        }
    }
}