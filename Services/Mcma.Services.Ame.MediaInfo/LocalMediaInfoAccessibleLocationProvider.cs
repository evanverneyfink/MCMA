using System.IO;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Server.Files;

namespace Mcma.Services.Ame.MediaInfo
{
    internal class LocalMediaInfoAccessibleLocationProvider : IMediaInfoAccessibleLocationProvider
    {
        /// <summary>
        /// Gets a url for a given <see cref="Locator"/> that's accessible by MediaInfo on the local machine
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public Task<string> GetMediaInfoAccessibleLocation(Locator locator)
        {
            return Task.FromResult(locator is LocalLocator local ? Path.Combine(local.FolderPath, local.FileName) : null);
        }
    }
}