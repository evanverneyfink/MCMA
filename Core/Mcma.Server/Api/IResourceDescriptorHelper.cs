using Mcma.Core;

namespace Mcma.Server.Api
{
    public interface IResourceDescriptorHelper
    {
        /// <summary>
        /// Gets a resource descriptor from the path of a resource
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        ResourceDescriptor GetResourceDescriptor(string path, string baseUrl = null);

        /// <summary>
        /// Gets the path to a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        string GetUrlPath(ResourceDescriptor resourceDescriptor);
    }
}