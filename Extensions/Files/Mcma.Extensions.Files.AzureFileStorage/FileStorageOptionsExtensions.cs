using System;

namespace Mcma.Extensions.Files.AzureFileStorage
{
    public static class FileStorageOptionsExtensions
    {
        public const string AccountNameKey = "StorageAccountName";
        public const string KeyValueKey = "StorageAccountKeyValue";

        public static FileStorageOptions FromEnvironmentVariables(this FileStorageOptions options)
        {
            options.AccountName = Environment.GetEnvironmentVariable(AccountNameKey);
            options.KeyValue = Environment.GetEnvironmentVariable(KeyValueKey);
            return options;
        }
    }
}