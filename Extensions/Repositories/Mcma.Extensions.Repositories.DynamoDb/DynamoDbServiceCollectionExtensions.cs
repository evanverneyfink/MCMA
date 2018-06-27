using System;
using Mcma.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Extensions.Repositories.DynamoDb
{
    public static class DynamoDbServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DynamoDB as the repository service behind FIMS
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddDynamoDbMcmaRepository(this IServiceCollection serviceCollection, Action<DynamoDbOptions> configureOptions = null)
        {
            if (configureOptions != null)
                serviceCollection.Configure(configureOptions);

            return serviceCollection.AddSingleton<IDynamoDbTableConfigProvider, DefaultDynamoDbTableConfigProvider>()
                                    .AddScoped(typeof(IRepository), typeof(DynamoDbRepository));
        }
    }
}