using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace Mcma.Azure.FileStorage
{
    public class FileStorageOptions : AzureOptions
    {
        /// <summary>
        /// Creates a file client
        /// </summary>
        /// <returns></returns>
        public CloudFileClient CreateFileClient() => new CloudStorageAccount(StorageCredentials, true).CreateCloudFileClient();
    }
}