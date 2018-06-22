using System;
using System.IO;
using System.Threading.Tasks;
using Mcma.Azure.FileStorage;
using Mcma.Core.Model;
using Mcma.Services.Ame.MediaInfo;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.File;

namespace Mcma.Azure.Services.Ame.MediaInfo
{
    public class AzureMediaInfoAccessibleLocationProvider : IMediaInfoAccessibleLocationProvider
    {
        /// <summary>
        /// Instantiates an <see cref="AzureMediaInfoAccessibleLocationProvider"/>
        /// </summary>
        /// <param name="options"></param>
        public AzureMediaInfoAccessibleLocationProvider(IOptions<FileStorageOptions> options)
        {
            FileClient = (options?.Value ?? new FileStorageOptions()).CreateFileClient();
        }

        /// <summary>
        /// Gets the file client
        /// </summary>
        private CloudFileClient FileClient { get; }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public async Task<string> GetMediaInfoAccessibleLocation(Locator locator)
        {
            // ensure we have the right type of locator
            if (locator == null)
                throw new ArgumentNullException(nameof(locator));
            if (!(locator is AzureFileStorageLocator azureLocator))
                throw new Exception($"Expected an Azure File Storage locator, but got a {locator.GetType().Name}.");

            // get the file in storage
            var file = FileClient.GetFile(azureLocator);

            // get path for temp file
            var tmpFile = Path.Combine(Path.GetTempPath(), azureLocator.FileName);

            // download from storage to temp location
            await file.DownloadToFileAsync(tmpFile, FileMode.Create);
            
            // return the new temp file location
            return tmpFile;
        }
    }
}