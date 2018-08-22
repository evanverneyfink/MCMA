using System.Threading.Tasks;
using Mcma.Azure.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Mcma.Azure.Services.Ame.MediaInfo.Worker
{
    public static class Functions
    {
        [FunctionName(nameof(Worker))]
        public static Task<IActionResult> Worker([HttpTrigger] HttpRequest request, [Inject] IMcmaAzureWorker worker)
        {
            worker.DoWork(request);

            return Task.FromResult<IActionResult>(new OkResult());
        }
    }
}