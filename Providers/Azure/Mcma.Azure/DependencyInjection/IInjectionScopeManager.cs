using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.DependencyInjection
{
    public interface IInjectionScopeManager
    {
        /// <summary>
        /// Begins a scope when a function starts
        /// </summary>
        /// <param name="id"></param>
        IServiceScope BeginScope(Guid id);

        /// <summary>
        /// Ends a scope when a function ends
        /// </summary>
        /// <param name="id"></param>
        void EndScope(Guid id);

        /// <summary>
        /// Gets the service provider for a function instance
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IServiceProvider GetServiceProvider(Guid id);
    }
}