using System;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public static class TableStorageOptionsExtensions
    {
        public const string AccountNameKey = "StorageAccountName";
        public const string KeyValueKey = "StorageAccountKeyValue";

        public static TableStorageOptions FromEnvironmentVariables(this TableStorageOptions options)
        {
            options.AccountName = Environment.GetEnvironmentVariable(AccountNameKey);
            options.KeyValue = Environment.GetEnvironmentVariable(KeyValueKey);
            return options;
        }
    }
}