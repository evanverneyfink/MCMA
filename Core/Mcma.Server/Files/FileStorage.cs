using System;
using System.Threading.Tasks;
using Mcma.Core.Model;

namespace Mcma.Server.Files
{
    public abstract class FileStorage<T> : IFileStorage where T : Locator
    {
        /// <summary>
        /// Explicit implementation of save file that checks that the provided locator is of a supported type
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        Task<Locator> IFileStorage.WriteTextToFile(Locator locator, string fileName, string contents)
        {
            if (!(locator is T typedLocator))
                throw new Exception("Locator must be an AWS S3 locator.");

            return WriteTextToFile(typedLocator, fileName, contents);
        }

        /// <summary>
        /// Saves a file using the given locator
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        protected abstract Task<Locator> WriteTextToFile(T locator, string fileName, string contents);
    }
}