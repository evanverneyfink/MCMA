using System.IO;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Mcma.Aws.Lambda;
using Mcma.Aws.Lambda.ApiGatewayProxy;
using Mcma.Extensions.Files.S3;
using Mcma.Extensions.Repositories.DynamoDb;
using Mcma.Services.Ame.MediaInfo;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Mcma.Aws.Services.Ame.MediaInfo
{
    public class MediaInfoFunctions
    {
        /// <summary>
        /// A Lambda function to respond to API calls to create MediaInfo jobs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>The list of blogs</returns>
        public Task<APIGatewayProxyResponse> JobApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return LambdaApiGatewayProxy.Handle<LambdaWorkerFunctionInvocation>(
                request,
                context,
                builder =>
                    builder.Services
                           .AddDynamoDbMcmaRepository()
                           .AddS3FileStorage());
        }

        /// <summary>
        /// A Lambda function to run the MediaInfo worker
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void Worker(Stream input, ILambdaContext context)
        {
            LambdaWorker.Handle<MediaInfoWorker>(
                input,
                context,
                serviceBuilder =>
                {
                    serviceBuilder.Services
                                  .AddDynamoDbMcmaRepository()
                                  .AddS3FileStorage();

                    serviceBuilder.AddAwsMediaInfo();
                });
        }
    }
}
