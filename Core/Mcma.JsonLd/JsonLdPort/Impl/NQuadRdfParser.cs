using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace JsonLD.Impl
{
    public class NQuadRdfParser : IRdfParser
    {
        /// <exception cref="JsonLD.Core.JsonLdError"></exception>
        public virtual RdfDataset Parse(JToken input)
        {
            if (input.Type == JTokenType.String)
                return RdfDatasetUtils.ParseNQuads((string)input);
            throw new JsonLdError(JsonLdError.Error.InvalidInput,
                                  "NQuad Parser expected string input."
            );
        }
    }
}