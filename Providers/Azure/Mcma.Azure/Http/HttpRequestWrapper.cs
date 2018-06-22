using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Server.Api;
using Microsoft.AspNetCore.Http;

namespace Mcma.Azure.Http
{
    public class HttpRequestWrapper : IRequest
    {
        /// <summary>
        /// Instantiates a <see cref="HttpRequestWrapper"/>
        /// </summary>
        /// <param name="request"></param>
        public HttpRequestWrapper(HttpRequest request)
        {
            Request = request;
        }

        /// <summary>
        /// Gets the underlying request
        /// </summary>
        private HttpRequest Request { get; }

        /// <summary>
        /// Gets the HTTP method
        /// </summary>
        public string Method => Request.Method;

        /// <summary>
        /// Gets the path of the request
        /// </summary>
        public string Path => Request.Path;

        /// <summary>
        /// Gets the dictionary of query string parameters
        /// </summary>
        public IDictionary<string, string> QueryParameters => Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

        /// <summary>
        /// Gets the response to be sent back to the requester
        /// </summary>
        /// <returns></returns>
        public IResponse Response { get; } = new ActionResultResponse();

        /// <summary>
        /// Reads the body of the reqeust as JSON
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadBodyAsText() => await new StreamReader(Request.Body).ReadToEndAsync();
    }
}