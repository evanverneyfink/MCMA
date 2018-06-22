using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Server.Api;
using Microsoft.AspNetCore.Http;

namespace Mcma.WebApi
{
    public class WebApiRequest : IRequest
    {
        /// <summary>
        /// Instantiates a <see cref="WebApiRequest"/>
        /// </summary>
        /// <param name="httpContext"></param>
        public WebApiRequest(HttpContext httpContext)
        {
            HttpContext = httpContext;
            Response = new WebApiResponse(HttpContext.Response);
        }

        /// <summary>
        /// Gets the HTTP context
        /// </summary>
        private HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the HTTP method
        /// </summary>
        public string Method => HttpContext.Request.Method;

        /// <summary>
        /// Gets the path of the request
        /// </summary>
        public string Path => HttpContext.Request.Path;

        /// <summary>
        /// Gets the dictionary of query string parameters
        /// </summary>
        public IDictionary<string, string> QueryParameters => HttpContext.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

        /// <summary>
        /// Reads the body of the reqeust as JSON
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadBodyAsText()
        {
            using (var streamReader = new StreamReader(HttpContext.Request.Body))
                return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// Gets the response to be sent back to the requester
        /// </summary>
        /// <returns></returns>
        public IResponse Response { get; }
    }
}