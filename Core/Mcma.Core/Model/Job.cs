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

        public ExpandoObject JobInput { get; set; } = new ExpandoObject();

        public ExpandoObject JobOutput { get; set; } = new ExpandoObject();

        public bool TryGetInput(string name, out object input) => ((IDictionary<string, object>)JobInput).TryGetValue(name, out input);

        public bool TryGetOutput(string name, out object output) => ((IDictionary<string, object>)JobOutput).TryGetValue(name, out output);
    }
}