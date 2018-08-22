using Microsoft.AspNetCore.Http;

namespace Mcma.Azure
{
    public interface IMcmaAzureWorker
    {
        /// <summary>
        /// Executes a worker using the content provided in an HTTP request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        void DoWork(HttpRequest request);
    }
}