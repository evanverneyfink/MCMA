using JsonLD.Core;

namespace JsonLD.Impl
{
    public class NQuadTripleCallback : IJsonLdTripleCallback
    {
        public virtual object Call(RdfDataset dataset)
        {
            return RdfDatasetUtils.ToNQuads(dataset);
        }
    }
}