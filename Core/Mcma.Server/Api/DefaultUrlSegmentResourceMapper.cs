using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mcma.Core.Model;
using Pluralize.NET;

namespace Mcma.Server.Api
{
    public class DefaultUrlSegmentResourceMapper : IUrlSegmentResourceMapper
    {
        /// <summary>
        /// Instantiates a <see cref="DefaultUrlSegmentResourceMapper"/>
        /// </summary>
        /// <param name="logger"></param>
        public DefaultUrlSegmentResourceMapper(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the pluralizer - used to singularize type names
        /// </summary>
        private Pluralizer Pluralizer { get; } = new Pluralizer();

        /// <summary>
        /// Maps a type name to a resource type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public Type GetResourceType(string typeName, Type parentType = null)
        {
            Logger.Debug("Getting resource type for type name {0}...", typeName);

            // singularize the type
            var singular = Pluralizer.Singularize(typeName);

            Logger.Debug("Singular type name = {0}", singular);

            // ensure we're in Pascal case
            var pascalCase = $"{char.ToUpper(singular[0])}{singular.Substring(1)}";

            Logger.Debug("Pascal type name = {0}", pascalCase);

            // get namespace-qualified resource name
            var fullName = $"{typeof(Resource).Namespace}.{pascalCase}";

            Logger.Debug("Full type name = {0}", fullName);

            // get namespace-qualified resource name
            var assemblyQualifiedTypeName = $"{fullName}, {typeof(Resource).Assembly.FullName}";

            Logger.Debug("Assembly-qualified type name = {0}", assemblyQualifiedTypeName);

            // get the type using the full name
            var type = Type.GetType(assemblyQualifiedTypeName);

            // unrecognized or non-resource type
            if (type == null || !typeof(Resource).IsAssignableFrom(type))
            {
                Logger.Warning("Type with full name {0} not found in the AppDomain.", assemblyQualifiedTypeName);
                return null;
            }

            // if there's parent type OR it's a child collection on the parent type, return it
            return
                parentType == null || parentType.GetProperties().Any(p => IsChildCollectionOfType(p, type))
                    ? type
                    : null;
        }

        /// <summary>
        /// Gets the type name for a resource type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetResourceTypeName(Type type)
        {
            // pluralize the type name
            return Pluralizer.Pluralize(type.Name);
        }

        /// <summary>
        /// Checks if a property is a child collection of a given type
        /// </summary>
        /// <param name="p"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsChildCollectionOfType(PropertyInfo p, Type type)
        {
            return
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                p.PropertyType.GetGenericArguments().First() == type;
        }
    }
}