using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Server
{
    public static class ConsoleLoggerServiceCollectionExtensions
    {
        public static IServiceCollection AddConsoleLogger(this IServiceCollection services)
        {
            return services.AddSingleton<ILogger, ConsoleLogger>();
        }
    }
}