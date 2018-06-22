using System;

namespace Mcma.Core.Model
{
    public class Resource
    {
        public Resource()
        {
            Type = GetType().Name;
        }

        public string Id { get; set; }

        public string Type { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateModified { get; set; }
    }
}