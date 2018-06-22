using System;
using Microsoft.Azure.WebJobs.Description;

namespace Mcma.Azure.DependencyInjection
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectAttribute : Attribute
    {
    }
}
