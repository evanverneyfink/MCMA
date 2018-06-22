using Mcma.Services.Ame.MediaInfo;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public class AzureProcessLocator : IMediaInfoProcessLocator
    {
        /// <summary>
        /// Gets the path to the media info process
        /// </summary>
        /// <returns></returns>
        public string GetMediaInfoLocation()
        {
            return "binaries\\MediaInfo.exe";
        }
    }
}