using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Server.Files
{
    public interface IFileStorage
    {
        /// <summary>
        /// Saves a file to storage
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        Task<Locator> SaveFile(Locator locator, string fileName, string contents);
    }
}