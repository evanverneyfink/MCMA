using System;
using System.IO;
using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Server.Files
{
    public class LocalFileStorage : IFileStorage
    {
        /// <summary>
        /// Saves a file to storage
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        public Task<Locator> SaveFile(Locator locator, string fileName, string contents)
        {
            if (!(locator is LocalLocator localLocator))
                throw new Exception("Locator is not for a local file.");
            
            fileName = (localLocator.FileName ?? string.Empty) + fileName;

            // build path to output folder
            var filePath = Path.Combine(localLocator.FolderPath, fileName);

            // write file
            File.WriteAllText(filePath, contents);

            return Task.FromResult<Locator>(new LocalLocator {FolderPath = localLocator.FolderPath, FileName = fileName});
        }
    }
}