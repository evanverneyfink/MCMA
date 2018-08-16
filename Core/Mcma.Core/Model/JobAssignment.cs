using System.Dynamic;

namespace Mcma.Core.Model
{
    public class JobAssignment : Resource
    {
        public string JobProcess { get; set; }

        public string JobProcessStatus { get; set; }

        public string JobProcessStatusReason { get; set; }

        public ExpandoObject JobOutput { get; set; }
    }
}