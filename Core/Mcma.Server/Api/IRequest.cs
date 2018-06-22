using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcma.Server.Api
{
    public interface IRequest
    {
        /// <summary>
        /// Gets the HTTP method
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Gets the path of the request
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the dictionary of query string parameters
        /// </summary>
        IDictionary<string, string> QueryParameters { get; }

        /// <summary>
        /// Reads the body of the reqeust as JSON
        /// </summary>
        /// <returns></returns>
        Task<string> ReadBodyAsText();
        
        /// <summary>
        /// Gets the response to be sent back to the requester
        /// </summary>
        /// <returns></returns>
        IResponse Response { get; }
    }
}