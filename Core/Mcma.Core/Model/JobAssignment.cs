using System.Collections.Generic;

namespace Mcma.Core.Model
{
    public class JobAssignment : Resource
    {
        public string JobProcess { get; set; }

        public string JobProcessStatus { get; set; }

        public string JobProcessStatusReason { get; set; }

        public IDictionary<string, object> JobOutput { get; set; }
    }
}