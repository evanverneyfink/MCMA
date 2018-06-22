using System.Threading.Tasks;
using FluentAssertions;
using JsonLD.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsonLd.Tests
{
    [TestClass]
    public class DocumentLoaderTests
    {
        [TestMethod]
        public async Task ShouldLoadRemoteDocument()
        {
            var docLoader = new DocumentLoader();

            var remoteDoc = await docLoader.LoadDocumentAsync("https://dpzr6corrg.execute-api.us-east-1.amazonaws.com/dev/context/default");

            remoteDoc.DocumentUrl.Should().NotBeNullOrWhiteSpace();
            remoteDoc.Document.Should().NotBeNull();
            remoteDoc.ContextUrl.Should().NotBeNullOrWhiteSpace();
            remoteDoc.Context.Should().NotBeNull();
        }
    }
}
