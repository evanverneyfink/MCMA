using System;
using Mcma.Azure.Startup;
using Mcma.Extensions.Files.AzureFileStorage;
using Mcma.Extensions.Repositories.AzureTableStorage;
using Mcma.Server.Environment;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.Services.ServiceRegistry
{
    public class Startup : IStartup
    {
        public IServiceCollection Configure(IServiceCollection services)
            => services.AddMcmaResourceApi<Mcma.Services.ServiceRegistry.ServiceRegistry>(
                           configBuilder =>
                           {
                               Console.WriteLine($"Host name = {System.Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}");

                               configBuilder.Properties[nameof(EnvironmentExtensions.PublicUrl)] =
                                   $"https://{System.Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}";
                           })
                       .AddAzureTableStorageRepository(opts => opts.FromEnvironmentVariables())
                       .AddAzureFileStorage(opts => opts.FromEnvironmentVariables());
    }
}
