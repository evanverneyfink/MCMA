using System;

namespace Mcma.Server.Api
{
    public interface IUrlSegmentResourceMapper
    {
        /// <summary>
        /// Maps a type name to a resource type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        Type GetResourceType(string typeName, Type parentType = null);

        /// <summary>
        /// Gets the type name for a resource type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetResourceTypeName(Type type);
    }
}