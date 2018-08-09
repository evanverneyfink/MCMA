using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;

namespace Mcma.Extensions.Repositories.AzureTableStorage
{
    public static class StorageExceptionExtensions
    {
        public static string ToExtendedInfo(this StorageException storageException)
            => new StringBuilder()
               .AppendLine($"Request ID: {storageException.RequestInformation.ServiceRequestID}")
               .AppendLine($"Request Date: {storageException.RequestInformation.RequestDate}")
               .AppendLine($"Start: {storageException.RequestInformation.StartTime}")
               .AppendLine($"End: {storageException.RequestInformation.EndTime}")
               .AppendLine($"ETag: {storageException.RequestInformation.Etag}")
               .AppendLine($"MD5: {storageException.RequestInformation.ContentMd5}")
               .AppendLine($"Target Location: {storageException.RequestInformation.TargetLocation}")
               .AppendLine(
                   $"Response Code: ({storageException.RequestInformation.HttpStatusCode}) {storageException.RequestInformation.HttpStatusMessage}")
               .AppendLine($"Extended Error Code: {storageException.RequestInformation.ExtendedErrorInformation?.ErrorCode}")
               .AppendLine($"Extended Error Message: {storageException.RequestInformation.ExtendedErrorInformation?.ErrorMessage}")
               .AppendLine(
                   $"Extended Details: {string.Join(",", storageException.RequestInformation.ExtendedErrorInformation?.AdditionalDetails?.Select(kvp => $"{kvp.Key}={kvp.Value}") ?? new string[0])}")
               .AppendLine($"Exception: {storageException.RequestInformation.Exception}")
               .ToString();
    }
}