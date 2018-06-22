using Mcma.Server;
using Mcma.Server.Api;
using Mcma.Server.Environment;
using Mcma.Server.Files;
using Mcma.Server.LiteDb;
using Mcma.Services.Ame.MediaInfo;
using Mcma.Services.Jobs.WorkerFunctions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.WebApi.Services.Ame.MediaInfo
{
    public class Startup
    {
        /// <summary>
        /// Instantiates <see cref="Startup"/>
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <summary>
        /// Configures FIMS services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // set worker function name
            Configuration[nameof(WorkerFunctionEnvironmentExtensions.WorkerFunctionName)] = typeof(MediaInfoWorker).AssemblyQualifiedName;

            services.AddConsoleLogger()
                    .AddLocalFileStorage()
                    .AddLiteDb()
                    .AddLocalMediaInfo()
                    .AddMcmaInProcessWorkerFunctionWebApi<MediaInfoWorker>(Configuration);
        }

        /// <summary>
        /// Configures the app to be a FIMS resource API
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMcmaWebApi();
        }
    }
}
