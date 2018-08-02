using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Mcma.Azure;
using Mcma.Azure.DependencyInjection;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public static partial class Functions
    {
        [FunctionName(nameof(Worker))]
        public static async Task<IActionResult> Worker([HttpTrigger] HttpRequest request, [Inject] IMcmaAzureWorker worker)
        {
            await worker.DoWork(request);

            return new OkResult();
        }
    }
}