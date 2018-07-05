using System;

namespace Mcma.Extensions.Files.AzureFileStorage
{
    public static class FileStorageOptionsExtensions
    {
        public const string AccountNameKey = "AZURE_ACCT_NAME";
        public const string KeyValueKey = "AZURE_KEY_VALUE";

        public static FileStorageOptions FromEnvironmentVariables(this FileStorageOptions options)
        {
            options.AccountName = Environment.GetEnvironmentVariable(AccountNameKey);
            options.KeyValue = Environment.GetEnvironmentVariable(KeyValueKey);
            return options;
        }
    }
}