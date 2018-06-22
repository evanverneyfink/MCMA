using System.Collections.Generic;
using System.Dynamic;

namespace Mcma.Core.Model
{
    public class Job : Resource
    {
        public string JobStatus { get; set; }

        public string JobStatusReason { get; set; }
        
        public JobProfile JobProfile { get; set; }

        public AsyncEndpoint AsyncEndpoint { get; set; }

        public string JobProcess { get; set; }

        public IDictionary<string, object> JobInput { get; set; } = new Dictionary<string, object>();

        public IDictionary<string, object> JobOutput { get; set; } = new Dictionary<string, object>();
    }
}