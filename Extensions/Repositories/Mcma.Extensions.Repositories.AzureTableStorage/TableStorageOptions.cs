using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public class TableStorageOptions
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
        public CloudTableClient CreateTableClient() => new CloudStorageAccount(StorageCredentials, true).CreateCloudTableClient();
    }
}