using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Azure.DependencyInjection
{
    public class InjectBinding : IBinding
    {
        /// <summary>
        /// Instantiates a <see cref="InjectBinding"/>
        /// </summary>
        /// <param name="injectionScopeManager"></param>
        /// <param name="parameter"></param>
        public InjectBinding(IInjectionScopeManager injectionScopeManager, ParameterInfo parameter)
        {
            InjectionScopeManager = injectionScopeManager;
            Parameter = parameter;
        }

        /// <summary>
        /// Gets the service provider
        /// </summary>
        private IInjectionScopeManager InjectionScopeManager { get; }

        /// <summary>
        /// Gets the parameter that's bound
        /// </summary>
        private ParameterInfo Parameter { get; }

        /// <summary>
        /// Gets flag indicating this binding is defined via an attribute
        /// </summary>
        public bool FromAttribute => true;

        /// <summary>
        /// Binds the value by wrapping it in a <see cref="InjectValueProvider"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context) => Task.FromResult<IValueProvider>(new InjectValueProvider(value));

        /// <summary>
        /// Binds the value by resolving it from a <see cref="BindingContext"/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<IValueProvider> BindAsync(BindingContext context)
            => BindAsync(InjectionScopeManager.GetServiceProvider(context.FunctionInstanceId).GetRequiredService(Parameter.ParameterType), context.ValueContext);

        /// <summary>
        /// Gets a parameter descriptor using the underlying parameter 
        /// </summary>
        /// <returns></returns>
        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor {Type = "inject", Name = Parameter.Name};
    }
}