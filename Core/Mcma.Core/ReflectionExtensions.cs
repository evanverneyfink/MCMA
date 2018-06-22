using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mcma.Core
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Checks if a type can be assigned to an IEnumerable of a given item type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAssignableToEnumerableOf<T>(this Type type)
        {
            return type.IsGenericType &&
                   type.GenericTypeArguments.Length == 1 &&
                   typeof(T).IsAssignableFrom(type.GenericTypeArguments[0]) &&
                   typeof(IEnumerable<>).MakeGenericType(type.GenericTypeArguments[0])
                                        .IsAssignableFrom(type);
        }

        /// <summary>
        /// Checks if a type is an implementation of a generic interface type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsImplementationOfGenericInterface(this Type type, Type interfaceType)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        /// <summary>
        /// Checks if a type is a generic enumerable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsGenericList(this Type type)
        {
            return type.IsAssignableFrom(typeof(IList)) && type.IsImplementationOfGenericInterface(typeof(IEnumerable<>));
        }

        /// <summary>
        /// Checks if a type is a generic dictionary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsGenericDictionary(this Type type)
        {
            return type.IsImplementationOfGenericInterface(typeof(IDictionary<,>));
        }

        /// <summary>
        /// Gets the item type for a generic enumerable, either from the 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetGenericEnumerableItemType(this Type type)
        {
            var interfaceType = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (interfaceType.IsGenericTypeDefinition && type.IsGenericTypeDefinition)
                throw new Exception($"Type {type.Name} is an open generic type. Enumerable item type cannot be determined.");

            return interfaceType.IsGenericTypeDefinition
                       ? type.GenericTypeArguments[0]
                       : interfaceType.GenericTypeArguments[0];
        }
    }
}