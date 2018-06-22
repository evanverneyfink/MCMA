using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Azure.TableStorage
{
    public class TableStorageOptions : AzureOptions
    {
        /// <summary>
        /// Creates a file client
        /// </summary>
        /// <returns></returns>
        public CloudTableClient CreateTableClient() => new CloudStorageAccount(StorageCredentials, true).CreateCloudTableClient();
    }
}