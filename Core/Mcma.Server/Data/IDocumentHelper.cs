using System;
using Mcma.Core.Model;

namespace Mcma.Server.Data
{
    public interface IDocumentHelper
    {
        /// <summary>
        /// Gets a document from a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        dynamic GetDocument(Resource resource);

        /// <summary>
        /// Gets a resource from a document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        T GetResource<T>(dynamic document) where T : Resource, new();

        /// <summary>
        /// Gets a resource from a document
        /// </summary>
        /// <param name="type"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        Resource GetResource(Type type, dynamic document);
    }
}
