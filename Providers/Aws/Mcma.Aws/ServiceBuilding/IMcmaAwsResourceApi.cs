using Mcma.Server.Api;

namespace Mcma.Aws.ServiceBuilding
{
    public interface IMcmaAwsResourceApi : IMcmaService
    {
        /// <summary>
        /// Gets the request handler
        /// </summary>
        IRequestHandler RequestHandler { get; }
    }
}