using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Mcma.Extensions.Files.S3;
using Mcma.Core.Model;
using Mcma.Services.Ame.MediaInfo;
using Microsoft.Extensions.Options;

namespace Mcma.Aws.Services.Ame.MediaInfo
{
    public class S3MediaInfoAccessibleLocationProvider : IMediaInfoAccessibleLocationProvider
    {
        /// <summary>
        /// Instantiates a <see cref="S3MediaInfoAccessibleLocationProvider"/>
        /// </summary>
        /// <param name="options"></param>
        public S3MediaInfoAccessibleLocationProvider(IOptions<S3Options> options)
        {
            var region = options.Value?.RegionEndpoint;
            var creds = options.Value?.Credentials;
            S3 = creds != null ? new AmazonS3Client(creds, region) : new AmazonS3Client();
        }
        
        /// <summary>
        /// Gets the S3 client
        /// </summary>
        private IAmazonS3 S3 { get; }

        /// <summary>
        /// Gets an S3 url for a given <see cref="Locator"/>
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public Task<string> GetMediaInfoAccessibleLocation(Locator locator)
        {
            if (locator == null)
                throw new ArgumentNullException(nameof(locator));
            if (!(locator is AwsS3Locator s3Locator))
                throw new Exception($"Expected an S3 locator, but got a {locator.GetType().Name}.");

            // TODO: make this async somehow? not exposed by AWS SDK...
            var preSignedUrl = S3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = s3Locator.AwsS3Bucket,
                Key = s3Locator.AwsS3Key,
                Expires = DateTime.Now.AddMinutes(5)
            });

            return Task.FromResult(preSignedUrl);
        }
    }
}