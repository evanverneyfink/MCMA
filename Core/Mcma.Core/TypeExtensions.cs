using System;

namespace Mcma.Core
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the default value for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Default(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}