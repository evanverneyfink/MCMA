using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Services.Ame.MediaInfo
{
    public interface IMediaInfoAccessibleLocationProvider
    {
        /// <summary>
        /// Gets a url for a given <see cref="Locator"/> that's accessible by MediaInfo in the given context
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        Task<string> GetMediaInfoAccessibleLocation(Locator locator);
    }
}