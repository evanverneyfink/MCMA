using System.IO;
using Mcma.Server;
using Mcma.Server.Environment;
using Mcma.Services.Ame.MediaInfo;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public class AzureProcessLocator : IMediaInfoProcessLocator
    {
        /// <summary>
        /// Instantiates an <see cref="AzureProcessLocator"/>
        /// </summary>
        /// <param name="logger"></param>
        public AzureProcessLocator(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the path to the media info process
        /// </summary>
        /// <returns></returns>
        public string GetMediaInfoLocation()
        {
            return System.Environment.ExpandEnvironmentVariables(@"%home%\site\wwwroot\binaries\MediaInfo.exe");
        }
    }
}