using System;

namespace Mcma.Core.Model
{
    public class JobProcess : Resource
    {
        public Job Job { get; set; }

        public string JobAssignment { get; set; }

        public string JobProcessStatus { get; set; }

        public string JobProcessStatusReason { get; set; }

        public DateTime? JobStart { get; set; }

        public DateTime? JobEnd { get; set; }

        public TimeSpan? JobDuration => JobEnd.HasValue && JobStart.HasValue ? JobEnd.Value - JobStart.Value : default(TimeSpan?);
    }
}