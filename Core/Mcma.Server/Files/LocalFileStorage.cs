using System.IO;
using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Server.Files
{
    public class LocalFileStorage : FileStorage<LocalLocator>
    {
        /// <summary>
        /// Saves a file to storage
        /// </summary>
        /// <param name="localLocator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        protected override Task<Locator> WriteTextToFile(LocalLocator localLocator, string fileName, string contents)
        {   
            fileName = (localLocator.FileName ?? string.Empty) + fileName;

            // build path to output folder
            var filePath = Path.Combine(localLocator.FolderPath, fileName);

            // write file
            File.WriteAllText(filePath, contents);

            return Task.FromResult<Locator>(new LocalLocator {FolderPath = localLocator.FolderPath, FileName = fileName});
        }
    }
}