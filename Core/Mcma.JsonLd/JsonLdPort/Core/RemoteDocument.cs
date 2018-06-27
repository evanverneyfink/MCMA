using Newtonsoft.Json.Linq;

namespace JsonLD.Core
{
    public class RemoteDocument
    {
        internal JToken context;

        internal string contextUrl;

        internal JToken document;

        internal string documentUrl;

        public RemoteDocument(string url, JToken document)
            : this(url, document, null)
        {
        }

        public RemoteDocument(string url, JToken document, string context)
        {
            documentUrl = url;
            this.document = document;
            contextUrl = context;
        }

        public virtual string DocumentUrl
        {
            get => documentUrl;
            set => documentUrl = value;
        }

        public virtual JToken Document
        {
            get => document;
            set => document = value;
        }

        public virtual string ContextUrl
        {
            get => contextUrl;
            set => contextUrl = value;
        }

        public virtual JToken Context
        {
            get => context;
            set => context = value;
        }
    }
}