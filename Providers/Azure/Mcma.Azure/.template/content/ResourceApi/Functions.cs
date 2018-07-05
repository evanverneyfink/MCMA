using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Mcma.Azure;
using Mcma.Azure.DependencyInjection;

namespace McmaServiceTemplate
{
    public static class Functions
    {
        [FunctionName(nameof(ResourceApi))]
        public static Task<IActionResult> ResourceApi([HttpTrigger] HttpRequest request, [Inject] IMcmaAzureResourceApi resourceApi)
        {
            return resourceApi.HandleRequest(request);
        }
    }
}