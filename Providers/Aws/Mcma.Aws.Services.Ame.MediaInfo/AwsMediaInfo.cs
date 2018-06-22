using Mcma.Aws.ServiceBuilding;
using Mcma.Services.Ame.MediaInfo;

namespace Mcma.Aws.Services.Ame.MediaInfo
{
    public static class AwsMediaInfo
    {
        public static McmaAwsServiceBuilder AddAwsMediaInfo(this McmaAwsServiceBuilder serviceBuilder)
        {
            return serviceBuilder.With(services => services.AddMediaInfo<S3MediaInfoAccessibleLocationProvider, LambdaProcessLocator>());
        }
    }
}