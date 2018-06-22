using System.Threading.Tasks;
using Mcma.Azure.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Mcma.Azure.Services.Jobs.JobProcessor
{
    public static class JobProcessorFunctions
    {
        public static Task<IActionResult> ResourceApi([HttpTrigger] HttpRequest request, [Inject] IMcmaAzureResourceApi resourceApi)
        {
            return resourceApi.HandleRequest(request);
        }
    }
}
