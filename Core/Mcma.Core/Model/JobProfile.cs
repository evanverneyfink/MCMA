using System.Collections.Generic;

namespace Mcma.Core.Model
{
    public class JobProfile : Resource
    {
        public string Label { get; set; }

        public ICollection<JobParameter> HasInputParameter { get; set; }

        public ICollection<JobParameter> HasOutputParameter { get; set; }
        
        public ICollection<JobParameter> HasOptionalInputParameter { get; set; }
    }
}