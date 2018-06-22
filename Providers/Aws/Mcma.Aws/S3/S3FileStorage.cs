using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Mcma.Core.Model;
using Mcma.Server.Files;

namespace Mcma.Aws.S3
{
    public class S3FileStorage : FileStorage<AwsS3Locator>
    {
        /// <summary>
        /// Gets the S3 client
        /// </summary>
        private IAmazonS3 S3 { get; } = new AmazonS3Client();

        /// <summary>
        /// Saves a file by doing a put to the S3 API
        /// </summary>
        /// <param name="s3Locator"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        protected override async Task<Locator> WriteTextToFile(AwsS3Locator s3Locator, string fileName, string contents)
        {
            var objectKey = (s3Locator.AwsS3Key ?? string.Empty) + fileName;

            await S3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = s3Locator.AwsS3Bucket,
                Key = objectKey,
                ContentBody = contents
            });

            return new AwsS3Locator {AwsS3Bucket = s3Locator.AwsS3Bucket, AwsS3Key = objectKey};
        }
    }
}
