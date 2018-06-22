using Amazon;
using Amazon.Runtime;

namespace Mcma.Aws
{
    public abstract class AwsOptions
    {
        /// <summary>
        /// Gets or sets the region
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the access key
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the secret key
        /// </summary>
        public string SecretKey { get; set; }

        public RegionEndpoint RegionEndpoint => Region != null ? RegionEndpoint.GetBySystemName(Region) : null;

        /// <summary>
        /// Gets basic AWS credentials using the access key and secret key, if set
        /// </summary>
        public AWSCredentials Credentials => AccessKey != null && SecretKey != null ? new BasicAWSCredentials(AccessKey, SecretKey) : null;
    }
}