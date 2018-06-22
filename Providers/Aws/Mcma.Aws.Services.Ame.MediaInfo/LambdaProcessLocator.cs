using Mcma.Server.Environment;
using Mcma.Services.Ame.MediaInfo;

namespace Mcma.Aws.Services.Ame.MediaInfo
{
    public class LambdaProcessLocator : IMediaInfoProcessLocator
    {
        /// <summary>
        /// Instantiates a <see cref="LambdaProcessLocator"/>
        /// </summary>
        /// <param name="environment"></param>
        public LambdaProcessLocator(IEnvironment environment)
        {
            Environment = environment;
        }
        
        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Gets the path to the media info process
        /// </summary>
        /// <returns></returns>
        public string GetMediaInfoLocation()
        {
            Environment.Set("PATH", $"{Environment.Get<string>("PATH")}:{Environment.Get<string>("LAMBDA_TASK_ROOT")}");

            return "binaries/mediainfo";
        }
    }
}