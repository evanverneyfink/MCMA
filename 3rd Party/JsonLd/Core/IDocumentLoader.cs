using System.Threading.Tasks;

namespace JsonLD.Core
{
    public interface IDocumentLoader
    {
        Task<RemoteDocument> LoadDocumentAsync(string url);
    }
}