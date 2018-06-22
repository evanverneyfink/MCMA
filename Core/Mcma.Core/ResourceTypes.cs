using System;
using System.Collections.Generic;
using System.Linq;
using Mcma.Core.Model;

namespace Mcma.Core
{
    public static class ResourceTypes
    {
        private static List<Type> Types { get; } = new List<Type>();

        public static void Add<T>()
        {
            Add(typeof(T));
        }

        public static void Add(Type type)
        {
            if (!Types.Contains(type))
                Types.Add(type);
        }

        public static Type ToResourceType(this string name)
        {
            if (name == null)
                return null;

            // check for a manually added type
            var typeValue = Types.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                                                      t.FullName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                                                      t.AssemblyQualifiedName.Equals(name, StringComparison.OrdinalIgnoreCase));

            // try with the name as-is at first
            if (typeValue == null)
                typeValue = Type.GetType(name);

            // try assuming the core namespace
            if (typeValue == null)
                typeValue = Type.GetType($"{typeof(Resource).Namespace}.{name}");

            // try assuming the core assembly-qualified name
            if (typeValue == null)
                typeValue = Type.GetType($"{typeof(Resource).Assembly.GetName().Name}, {typeof(Resource).Namespace}.{name}");

            return typeValue;
        }

        public static string ToResourceTypeName(this Type type)
        {
            return type.Assembly.FullName == typeof(Resource).Assembly.FullName ? type.Name : type.AssemblyQualifiedName;
        }
    }
}
