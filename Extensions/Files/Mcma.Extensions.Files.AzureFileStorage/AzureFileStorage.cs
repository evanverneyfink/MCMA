using System.IO;
using System.Text;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Server.Files;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.File;

namespace Mcma.Extensions.Files.AzureFileStorage
{
    public class AzureFileStorage : FileStorage<AzureFileStorageLocator>
    {
        /// <summary>
        /// Get UTF-8 encoding without UTF-8 identifier emission
        /// </summary>
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        /// <summary>
        /// Instantiates an <see cref="AzureFileStorage"/>
        /// </summary>
        /// <param name="options"></param>
        public AzureFileStorage(IOptions<FileStorageOptions> options)
        {
            FileClient = (options?.Value ?? new FileStorageOptions()).CreateFileClient();
        }

        /// <summary>
        /// Gets the client for Azure file storage
        /// </summary>
        public CloudFileClient FileClient { get; }

        /// <summary>
        /// Saves a file to Azure File Storage
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        protected override async Task<Locator> WriteTextToFile(AzureFileStorageLocator locator, string fileName, string contents)
        {
            // create locator for upload location
            var uploadLocator = new AzureFileStorageLocator {Directory = locator.Directory, FileName = (locator.FileName ?? string.Empty) + fileName};

            // upload text to speciifed file location
            await FileClient.GetFile(uploadLocator).UploadFromStreamAsync(new MemoryStream(Utf8.GetByteCount(contents)));

            return uploadLocator;
        }
    }
}