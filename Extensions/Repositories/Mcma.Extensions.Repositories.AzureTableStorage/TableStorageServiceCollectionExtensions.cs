using System;
using Mcma.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public static class TableStorageServiceCollectionExtension
    {
        public static IServiceCollection AddAzureTableStorageRepository(this IServiceCollection services, Action<TableStorageOptions> configureOptions = null)
        {
            if (configureOptions != null)
                services.Configure(configureOptions);

            

            return
                services.AddScoped<IAzureStorageTableConfigProvider, DefaultAzureStorageTableConfigProvider>()
                        .AddScoped<IRepository, TableStorageRepository>();
        }
    }
}