using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Mcma.WebApi.Services.Ame.MediaInfo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .UseLaunchSettingsInDev()
                   .Build();
    }
}
