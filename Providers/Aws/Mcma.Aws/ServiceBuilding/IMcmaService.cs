using System;
using Mcma.Server;

namespace Mcma.Aws.ServiceBuilding
{
    public interface IMcmaService : IDisposable
    {
        /// <summary>
        /// Gets the logger
        /// </summary>
        ILogger Logger { get; }
    }
}