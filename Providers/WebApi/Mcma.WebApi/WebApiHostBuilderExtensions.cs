using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Mcma.WebApi
{
    public static class WebApiHostBuilderExtensions
    {
        private const string LaunchSettingsJsonFile = "Properties/launchSettings.json";

        /// <summary>
        /// Sets public url from local host's app builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseLaunchSettingsInDev(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((ctx, config) =>
            {
                if (ctx.HostingEnvironment.IsDevelopment())
                    config.AddJsonFile(LaunchSettingsJsonFile);
            });
        }
    }
}