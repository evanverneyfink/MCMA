using System.Linq;
using Mcma.Core;
using Mcma.Server.Environment;

namespace Mcma.Server.Api
{
    internal class DefaultResourceDescriptorHelper : IResourceDescriptorHelper
    {
        /// <summary>
        /// Instantiates a <see cref="DefaultResourceDescriptorHelper"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="environment"></param>
        /// <param name="urlSegmentResourceMapper"></param>
        public DefaultResourceDescriptorHelper(ILogger logger, IEnvironment environment, IUrlSegmentResourceMapper urlSegmentResourceMapper)
        {
            Logger = logger;
            Environment = environment;
            UrlSegmentResourceMapper = urlSegmentResourceMapper;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the environment
        /// </summary>
        private IEnvironment Environment { get; }

        /// <summary>
        /// Gets the url segment to resource mapper
        /// </summary>
        private IUrlSegmentResourceMapper UrlSegmentResourceMapper { get; }

        /// <summary>
        /// Gets a resource descriptor from the path of a resource
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public ResourceDescriptor GetResourceDescriptor(string path, string baseUrl = null)
        {
            // get the root path from the environment
            var root = $"/{Environment.RootPath() ?? string.Empty}";
            if (!path.StartsWith(root))
            {
                Logger.Warning("Received request to path that does not match configured root. Root = {0}, Path = {1}", root, path);
                return null;
            }

            // remove root from path
            path = path.Substring(root.Length);

            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.Warning("Request was made to the root url. Resource type not specified.");
                return null;
            }

            Logger.Debug("Path without root: {0}", path);

            // remove root and then split
            var pathParts = path.TrimStart('/').Split('/');

            var curType = UrlSegmentResourceMapper.GetResourceType(pathParts[0]);
            if (curType == null)
            {
                Logger.Warning("Invalid path part '{0}' in path {1}. Type not found.", pathParts[0], path);
                return null;
            }

            var cur = new ResourceDescriptor(curType) {Url = (baseUrl ?? Environment.PublicUrl()).TrimEnd('/') + $"/{pathParts[0]}"};

            // first should be an ID
            var isId = true;

            foreach (var pathPart in pathParts.Skip(1))
            {
                // if this is an ID segment, just 
                if (isId)
                {
                    cur.Id = pathPart;
                    cur.Url += $"/{pathPart}";
                }
                else
                {
                    // get the type as a collection of the current type
                    curType = UrlSegmentResourceMapper.GetResourceType(pathPart, curType);
                    if (curType == null)
                    {
                        Logger.Warning("Invalid path part '{0}' in path {1}. Type not found.", pathPart, path);
                        return null;
                    }

                    // create new with existing as parent
                    cur = new ResourceDescriptor(curType) {Parent = cur, Url = $"{cur.Url}/{pathPart}"};
                }

                isId = !isId;
            }

            return cur;
        }

        /// <summary>
        /// Gets the path to a resource
        /// </summary>
        /// <param name="resourceDescriptor"></param>
        /// <returns></returns>
        public string GetUrlPath(ResourceDescriptor resourceDescriptor)
        {
            // start with the type
            var path = $"/{UrlSegmentResourceMapper.GetResourceTypeName(resourceDescriptor.Type)}";

            // check if we have an ID
            if (resourceDescriptor.Id != null)
                path += $"/{resourceDescriptor.Id}";

            // recurse up the tree for child collections
            if (resourceDescriptor.Parent != null)
                path = GetUrlPath(resourceDescriptor.Parent) + path;

            return path;
        }
    }
}