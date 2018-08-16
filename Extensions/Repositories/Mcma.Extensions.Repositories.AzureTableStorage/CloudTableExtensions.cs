using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public static class CloudTableExtensions
    {
        public static async Task<dynamic> ExecuteRetrieveAsync(this CloudTable table,
                                                               IAzureStorageTableConfigProvider tableConfigProvider,
                                                               string partitionKey,
                                                               string rowKey)
        {
            var result =
                await table.ExecuteAsync(
                    TableOperation.Retrieve(partitionKey,
                                            rowKey,
                                            (pKey, rKey, timestamp, properties, etag) =>
                                                new ResourceTableEntity(pKey, rKey, timestamp, properties, etag)));

            return (result.Result as ResourceTableEntity)?.Resource;
        }

        public static async Task<IEnumerable<dynamic>> ExecuteQueryAsync(this CloudTable table, IAzureStorageTableConfigProvider tableConfigProvider, TableQuery query)
        {
            var results = new List<dynamic>();

            TableContinuationToken continuationToken = null;
            do
            {
                // execute query and resolve results to ResourceTableEntity objects
                var result =
                    await table.ExecuteQuerySegmentedAsync(
                        query,
                        (partitionKey, rowKey, timestamp, properties, etag) => new ResourceTableEntity(partitionKey, rowKey, timestamp, properties, etag),
                        continuationToken);

                // add resource docs to results
                results.AddRange(result.Results.Select(e => e.Resource));

                continuationToken = result.ContinuationToken;
            }
            while (continuationToken != null);

            return results;
        }
    }
}