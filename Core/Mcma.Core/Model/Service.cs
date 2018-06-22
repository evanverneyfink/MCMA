using System;
using System.Collections.Generic;

namespace Mcma.Core.Model
{
    public class Service : Resource
    {
        public string Label { get; set; }

        public ICollection<ServiceResource> HasResource { get; set; }

        public ICollection<Type> AcceptsJobType { get; set; }

        public ICollection<JobProfile> AcceptsJobProfile { get; set; }

        public ICollection<Locator> InputLocation { get; set; }

        public ICollection<Locator> OutputLocation { get; set; }
    }
}
