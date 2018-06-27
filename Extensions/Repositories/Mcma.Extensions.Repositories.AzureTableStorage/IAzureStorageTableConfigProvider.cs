using System;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public interface IAzureStorageTableConfigProvider
    {
        /// <summary>
        /// Gets flag indicating if tables should be created if they don't exist
        /// </summary>
        bool CreateIfNotExists { get; }

        /// <summary>
        /// Gets the table name for a type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        string GetTableName(string typeName);

        /// <summary>
        /// Gets the table name for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetTableName(Type type);

        /// <summary>
        /// Gets the name of the hash key for a table of the specified type
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetPartitionKeyFieldName(string tableName);

        /// <summary>
        /// Gets the name of the sort key for a table of the specified type
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetRowKeyFieldName(string tableName);

        /// <summary>
        /// Gets the name of the resource field
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetResourceFieldName(string tableName);
    }
}