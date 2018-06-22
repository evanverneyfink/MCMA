using System.IO;
using System.Reflection;

namespace Mcma.Services.Ame.MediaInfo
{
    internal class LocalWindowsProcessLocator : IMediaInfoProcessLocator
    {
        /// <summary>
        /// Gets the path to the media info process
        /// </summary>
        /// <returns></returns>
        public string GetMediaInfoLocation()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mediainfo.exe");
        }
    }
}