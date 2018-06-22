using System;

namespace Mcma.Server.Business
{
    public interface IResourceHandlerRegistry
    {
        /// <summary>
        /// Checks if a resource type is supported
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsSupported(Type type);

        /// <summary>
        /// Gets the resource handler for a given resource type
        /// </summary>
        IResourceHandler Get(Type type);
    }
}