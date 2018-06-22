using System.Threading.Tasks;
using Mcma.Azure.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public static class MediaInfoFunctions
    {
        /// <summary>
        /// A Lambda function to respond to API calls to create MediaInfo jobs
        /// </summary>
        /// <param name="resourceApi"></param>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public static async Task<IActionResult> ResourceApi([HttpTrigger] HttpRequest request, [Inject] IMcmaAzureResourceApi resourceApi)
        {
            return await resourceApi.HandleRequest(request);
        }

        /// <summary>
        /// A Lambda function to run the MediaInfo worker
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<IActionResult> Worker([HttpTrigger] HttpRequest request, [Inject] IMcmaAzureWorker worker)
        {
            await worker.DoWork(request);
            return new OkResult();
        }
    }
}