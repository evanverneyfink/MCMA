using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Mcma.Azure.DependencyInjection
{
    public class InjectValueProvider : IValueProvider
    {
        /// <summary>
        /// Instantiates a <see cref="InjectValueProvider"/>
        /// </summary>
        /// <param name="value"></param>
        public InjectValueProvider(object value) => Value = value;

        /// <summary>
        /// Gets the underlying value object
        /// </summary>
        private object Value { get; }

        /// <summary>
        /// Gets the type from the underlying value
        /// </summary>
        public Type Type => Value?.GetType();

        /// <summary>
        /// Gets the underlying value
        /// </summary>
        /// <returns></returns>
        public Task<object> GetValueAsync() => Task.FromResult(Value);

        /// <summary>
        /// Gets the string value of the underlying object
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString() => Value?.ToString();
    }
}