using System.Net.Http;
using Mcma.Server.Api;
using Microsoft.AspNetCore.Mvc;

namespace Mcma.Azure.Http
{
    public interface IActionResultResponse : IResponse
    {
        /// <summary>
        /// Gets the response as an <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <returns></returns>
        IActionResult AsActionResult();
    }
}