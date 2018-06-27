using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;

namespace Mcma.Extensions.Files.AzureFileStorage
{
    public class FileStorageOptions
    {
        /// <summary>
        /// Gets or sets the Azure account name
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the Azure account key value
        /// </summary>
        public string KeyValue { get; set; }

        /// <summary>
        /// Gets Azure storage credentials
        /// </summary>
        /// <returns></returns>
        public StorageCredentials StorageCredentials
            => AccountName != null
                   ? (KeyValue != null
                          ? new StorageCredentials(AccountName, KeyValue)
                          : new StorageCredentials(AccountName))
                   : new StorageCredentials();

        /// <summary>
        /// Creates a file client
        /// </summary>
        /// <returns></returns>
        public CloudFileClient CreateFileClient() => new CloudStorageAccount(StorageCredentials, true).CreateCloudFileClient();
    }
}