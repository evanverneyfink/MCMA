using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;

namespace Mcma.Azure
{
    public abstract class AzureOptions
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
    }
}