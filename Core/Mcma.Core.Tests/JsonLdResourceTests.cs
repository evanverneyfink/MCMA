using Mcma.JsonLd;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JsonLdProcessor = Mcma.JsonLd.JsonLdProcessor;

/*
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Linq;
*/

namespace Mcma.Core.Tests
{
    [TestClass]
    public class JsonLdResourceTests
    {
        private IJsonLdContextManager JsonLdContextManager { get; set; }

        private IJsonLdResourceHelper JsonLdResourceHelper { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            JsonLdContextManager = new JsonLdContextManager(null);
            JsonLdResourceHelper =
                new JsonLdResourceHelper(JsonLdContextManager, new JsonLdProcessor(null, new CachedDocumentLoader(JsonLdContextManager)));
        }
        /*
        [TestMethod]
        public async Task AllowsCreatingAJobProfile()
        {
            var jobProfile = new JobProfile("ExtractThumbnail",
                                            new JArray
                                            {
                                                new JobParameter("mcma:inputFile", "mcma:Locator"),
                                                new JobParameter("mcma:outputLocation", "mcma:Locator")
                                            },
                                            new JArray
                                            {
                                                new JobParameter("mcma:outputFile", "mcma:Locator")
                                            },
                                            new JArray
                                            {
                                                new JobParameter("ebucore:width"),
                                                new JobParameter("ebucore:height")
                                            });

            var output = await JsonLdResourceHelper.GetJsonFromResource(jobProfile, new JObject());

            output.Should().NotBeNull();

            output["@type"].Value<string>().Should().Be("http://mcma.tv#JobProfile");
            output["http://www.w3.org/2000/01/rdf-schema#label"].Value<string>().Should().Be("ExtractThumbnail");

            output["http://mcma.tv#hasInputParameter"].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"].Count().Should().Be(2);
            output["http://mcma.tv#hasInputParameter"][0].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"][0]["@type"].Value<string>().Should().Be("http://mcma.tv#JobParameter");
            output["http://mcma.tv#hasInputParameter"][0]["http://mcma.tv#jobParameterType"].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"][0]["http://mcma.tv#jobParameterType"]["@id"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasInputParameter"][0]["http://mcma.tv#jobProperty"].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"][0]["http://mcma.tv#jobProperty"]["@id"].Value<string>().Should().Be("http://mcma.tv#inputFile");
            output["http://mcma.tv#hasInputParameter"][1].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"][1]["@type"].Value<string>().Should().Be("http://mcma.tv#JobParameter");
            output["http://mcma.tv#hasInputParameter"][1]["http://mcma.tv#jobParameterType"].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"][1]["http://mcma.tv#jobParameterType"]["@id"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasInputParameter"][1]["http://mcma.tv#jobProperty"].Should().NotBeNull();
            output["http://mcma.tv#hasInputParameter"][1]["http://mcma.tv#jobProperty"]["@id"].Value<string>().Should().Be("http://mcma.tv#outputLocation");

            output["http://mcma.tv#hasOptionalInputParameter"].Should().NotBeNull();
            output["http://mcma.tv#hasOptionalInputParameter"].Count().Should().Be(2);
            output["http://mcma.tv#hasOptionalInputParameter"][0].Should().NotBeNull();
            output["http://mcma.tv#hasOptionalInputParameter"][0]["@type"].Value<string>().Should().Be("http://mcma.tv#JobParameter");
            output["http://mcma.tv#hasOptionalInputParameter"][0]["http://mcma.tv#jobProperty"].Should().NotBeNull();
            output["http://mcma.tv#hasOptionalInputParameter"][0]["http://mcma.tv#jobProperty"]["@id"].Value<string>().Should().Be("http://www.ebu.ch/metadata/ontologies/ebucore/ebucore#width");
            output["http://mcma.tv#hasOptionalInputParameter"][1].Should().NotBeNull();
            output["http://mcma.tv#hasOptionalInputParameter"][1]["@type"].Value<string>().Should().Be("http://mcma.tv#JobParameter");
            output["http://mcma.tv#hasOptionalInputParameter"][1]["http://mcma.tv#jobProperty"].Should().NotBeNull();
            output["http://mcma.tv#hasOptionalInputParameter"][1]["http://mcma.tv#jobProperty"]["@id"].Value<string>().Should().Be("http://www.ebu.ch/metadata/ontologies/ebucore/ebucore#height");

            output["http://mcma.tv#hasOutputParameter"].Should().NotBeNull();
            output["http://mcma.tv#hasOutputParameter"]["@type"].Value<string>().Should().Be("http://mcma.tv#JobParameter");
            output["http://mcma.tv#hasOutputParameter"]["http://mcma.tv#jobParameterType"].Should().NotBeNull();
            output["http://mcma.tv#hasOutputParameter"]["http://mcma.tv#jobParameterType"]["@id"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasOutputParameter"]["http://mcma.tv#jobProperty"].Should().NotBeNull();
            output["http://mcma.tv#hasOutputParameter"]["http://mcma.tv#jobProperty"]["@id"].Value<string>().Should().Be("http://mcma.tv#outputFile");
        }

        [TestMethod]
        public async Task AllowsCreatingAService()
        {
            var service = new Service("FFmpeg TransformService",
                                      new JArray
                                      {
                                          new ServiceResource("mcma:JobAssignment", "http://transformServiceUrl/JobAssignment")
                                      },
                                      "mcma:TransformJob",
                                      new JArray
                                      {
                                          "http://urlToExtractThumbnailJobProfile/",
                                          "http://urlToCreateProxyJobProfile/"
                                      },
                                      new JArray
                                      {
                                          new Locator(new JObject {["awsS3Bucket"] = "private-repo.mcma.tv"})
                                      },
                                      new JArray
                                      {
                                          new Locator(new JObject {["awsS3Bucket"] = "private-repo.mcma.tv"})
                                      });

            var output = await JsonLdResourceHelper.GetJsonFromResource(service, new JObject());
            
            output["@type"].Value<string>().Should().Be("http://mcma.tv#Service");
            output["http://www.w3.org/2000/01/rdf-schema#label"].Value<string>().Should().Be("FFmpeg TransformService");

            output["http://mcma.tv#hasServiceResource"].Should().NotBeNull();
            output["http://mcma.tv#hasServiceResource"]["@type"].Value<string>().Should().Be("http://mcma.tv#ServiceResource");
            output["http://mcma.tv#hasServiceResource"]["http://mcma.tv#resourceType"].Should().NotBeNull();
            output["http://mcma.tv#hasServiceResource"]["http://mcma.tv#resourceType"]["@id"].Value<string>().Should().Be("http://mcma.tv#JobAssignment");
            output["http://mcma.tv#hasServiceResource"]["http://mcma.tv#httpEndpoint"].Should().NotBeNull();
            output["http://mcma.tv#hasServiceResource"]["http://mcma.tv#httpEndpoint"]["@type"].Value<string>().Should().Be("http://www.w3.org/2001/XMLSchema#anyURI");
            output["http://mcma.tv#hasServiceResource"]["http://mcma.tv#httpEndpoint"]["@value"].Value<string>().Should().Be("http://transformServiceUrl/JobAssignment");

            output["http://mcma.tv#acceptsJobProfile"].Should().NotBeNull();
            output["http://mcma.tv#acceptsJobProfile"].Count().Should().Be(2);
            output["http://mcma.tv#acceptsJobProfile"][0].Should().NotBeNull();
            output["http://mcma.tv#acceptsJobProfile"][0]["@id"].Value<string>().Should().Be("http://urlToExtractThumbnailJobProfile/");
            output["http://mcma.tv#acceptsJobProfile"][1].Should().NotBeNull();
            output["http://mcma.tv#acceptsJobProfile"][1]["@id"].Value<string>().Should().Be("http://urlToCreateProxyJobProfile/");

            output["http://mcma.tv#acceptsJobType"].Should().NotBeNull();
            output["http://mcma.tv#acceptsJobType"]["@id"].Value<string>().Should().Be("http://mcma.tv#TransformJob");

            output["http://mcma.tv#hasJobInputLocation"].Should().NotBeNull();
            output["http://mcma.tv#hasJobInputLocation"]["@type"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasJobInputLocation"]["http://mcma.tv#amazonWebServicesS3Bucket"].Value<string>().Should().Be("private-repo.mcma.tv");

            output["http://mcma.tv#hasJobOutputLocation"].Should().NotBeNull();
            output["http://mcma.tv#hasJobOutputLocation"]["@type"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasJobOutputLocation"]["http://mcma.tv#amazonWebServicesS3Bucket"].Value<string>().Should().Be("private-repo.mcma.tv");
        }

        [TestMethod]
        public async Task AllowsCreatingATransformJob()
        {
            var transformJob = new TransformJob("http://urlToExtractThumbnailJobProfile/",
                                                new JobParameterBag(
                                                    new JObject
                                                    {
                                                        ["mcma:inputFile"] = new JObject
                                                        {
                                                            ["type"] = "mcma:Locator",
                                                            ["awsS3Bucket"] = "private-repo.mcma.tv",
                                                            ["awsS3Key"] = "media-file.mp4"
                                                        },
                                                        ["mcma:outputLocation"] = new JObject
                                                        {
                                                            ["type"] = "mcma:Locator",
                                                            ["awsS3Bucket"] = "private-repo.mcma.tv",
                                                            ["awsS3Key"] = "thumbnails/"
                                                        }
                                                    }),
                                                new AsyncEndpoint("http://urlForJobSuccess", "http://urlForJobFailed"));

            var output = await JsonLdResourceHelper.GetJsonFromResource(transformJob, new JObject());

            output["@type"].Value<string>().Should().Be("http://mcma.tv#TransformJob");

            output["http://mcma.tv#hasJobProfile"].Should().NotBeNull();
            output["http://mcma.tv#hasJobProfile"]["@id"].Value<string>().Should().Be("http://urlToExtractThumbnailJobProfile/");

            output["http://mcma.tv#hasJobInput"].Should().NotBeNull();
            output["http://mcma.tv#hasJobInput"]["@type"].Value<string>().Should().Be("http://mcma.tv#JobParameterBag");
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#inputFile"].Should().NotBeNull();
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#inputFile"]["@type"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#inputFile"]["http://mcma.tv#amazonWebServicesS3Bucket"]
                .Value<string>().Should().Be("private-repo.mcma.tv");
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#inputFile"]["http://mcma.tv#amazonWebServicesS3Key"]
                .Value<string>().Should().Be("media-file.mp4");

            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#outputLocation"].Should().NotBeNull();
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#outputLocation"]["@type"].Value<string>().Should().Be("http://mcma.tv#Locator");
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#outputLocation"]["http://mcma.tv#amazonWebServicesS3Bucket"]
                .Value<string>().Should().Be("private-repo.mcma.tv");
            output["http://mcma.tv#hasJobInput"]["http://mcma.tv#outputLocation"]["http://mcma.tv#amazonWebServicesS3Key"]
                .Value<string>().Should().Be("thumbnails/");

            output["http://mcma.tv#hasJobStatus"].Should().NotBeNull();
            output["http://mcma.tv#hasJobStatus"]["@type"].Value<string>().Should().Be("http://mcma.tv#JobStatus");
            output["http://mcma.tv#hasJobStatus"]["@value"].Value<string>().Should().Be("New");

            output["http://mcma.tv#hasAsyncEndpoint"].Should().NotBeNull();
            output["http://mcma.tv#hasAsyncEndpoint"]["@type"].Value<string>().Should().Be("http://mcma.tv#AsyncEndpoint");
            output["http://mcma.tv#hasAsyncEndpoint"]["http://mcma.tv#asyncEndpointFailure"].Should().NotBeNull();
            output["http://mcma.tv#hasAsyncEndpoint"]["http://mcma.tv#asyncEndpointFailure"]["@type"]
                .Value<string>().Should().Be("http://www.w3.org/2001/XMLSchema#anyURI");
            output["http://mcma.tv#hasAsyncEndpoint"]["http://mcma.tv#asyncEndpointFailure"]["@value"]
                .Value<string>().Should().Be("http://urlForJobFailed");
            output["http://mcma.tv#hasAsyncEndpoint"]["http://mcma.tv#asyncEndpointSuccess"].Should().NotBeNull();
            output["http://mcma.tv#hasAsyncEndpoint"]["http://mcma.tv#asyncEndpointSuccess"]["@type"]
                .Value<string>().Should().Be("http://www.w3.org/2001/XMLSchema#anyURI");
            output["http://mcma.tv#hasAsyncEndpoint"]["http://mcma.tv#asyncEndpointSuccess"]["@value"]
                .Value<string>().Should().Be("http://urlForJobSuccess");
        }

        [TestMethod]
        public async Task AllowsCreatingAJobProcess()
        {
            var jobProcess = new JobProcess("http://urlToTransformJob");

            var output = await JsonLdResourceHelper.GetJsonFromResource(jobProcess, new JObject());

            output["@type"].Value<string>().Should().Be("http://mcma.tv#JobProcess");

            output["http://mcma.tv#hasJob"].Should().NotBeNull();
            output["http://mcma.tv#hasJob"]["@id"].Value<string>().Should().Be("http://urlToTransformJob");

            output["http://mcma.tv#hasJobProcessStatus"].Should().NotBeNull();
            output["http://mcma.tv#hasJobProcessStatus"]["@type"].Value<string>().Should().Be("http://mcma.tv#JobProcessStatus");
            output["http://mcma.tv#hasJobProcessStatus"]["@value"].Value<string>().Should().Be("New");
        }

        [TestMethod]
        public async Task AllowsCreatingAJobAssignment()
        {
            var jobAssignment = new JobAssignment("http://urlToJobProcess");

            var output = await JsonLdResourceHelper.GetJsonFromResource(jobAssignment, new JObject());

            output["@type"].Value<string>().Should().Be("http://mcma.tv#JobAssignment");

            output["http://mcma.tv#hasJobProcess"].Should().NotBeNull();
            output["http://mcma.tv#hasJobProcess"]["@id"].Value<string>().Should().Be("http://urlToJobProcess");

            output["http://mcma.tv#hasJobProcessStatus"].Should().NotBeNull();
            output["http://mcma.tv#hasJobProcessStatus"]["@type"].Value<string>().Should().Be("http://mcma.tv#JobProcessStatus");
            output["http://mcma.tv#hasJobProcessStatus"]["@value"].Value<string>().Should().Be("New");
        }

        [TestMethod]
        public async Task AllowsGettingAService()
        {
            var resource =
                await JsonLdResourceHelper.GetResourceFromJson(JToken.Parse(File.ReadAllText("service.json")),
                                                               typeof(Service));

            resource.Should().NotBeNull();
            resource.Should().BeOfType<Service>();
        }
        */
    }
}
