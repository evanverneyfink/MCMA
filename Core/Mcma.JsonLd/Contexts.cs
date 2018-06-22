using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace Mcma.JsonLd
{
    public static class Contexts
    {
        public static class Minimal
        {
            /// <summary>
            /// Gets the url for the minimal context
            /// </summary>
            public const string Url = "http://mcma.tv/context/minimal";

            /// <summary>
            /// Gets the minimal context
            /// </summary>
            public static readonly Context Context =
                new Context(
                    new JObject
                    {
                        ["@context"] = new JObject
                        {
                            ["dc"] = "http://purl.org/dc/elements/1.1/",
                            ["@default"] = "urn:ebu:metadata-schema:ebuCore_2012",
                            ["ebu"] = "http://ebu.org/nar-extensions/",
                            ["ebucore"] = "http://www.ebu.ch/metadata/ontologies/ebucore/ebucore#",
                            ["mcma"] = "http://mcma.tv#",
                            ["owl"] = "http://www.w3.org/2002/07/owl#",
                            ["rdf"] = "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
                            ["rdfs"] = "http://www.w3.org/2000/01/rdf-schema#",
                            ["skos"] = "http://www.w3.org/2004/02/skos/core#",
                            ["xsd"] = "http://www.w3.org/2001/XMLSchema#",
                            ["xsi"] = "http://www.w3.org/2001/XMLSchema-instance",

                            ["id"] = "@id",
                            ["type"] = "@type"
                        }
                    });
        }

        public static class Default
        {
            /// <summary>
            /// Gets the url for the default context
            /// </summary>
            public const string Url = "http://mcma.tv/context/default";

            /// <summary>
            /// Gets the default context
            /// </summary>
            public static readonly Context Context =
                new Context(
                    new JObject
                    {
                        ["@context"] = new JObject
                        {

                            // Namespace abbreviations

                            ["ebucore"] = "http://www.ebu.ch/metadata/ontologies/ebucore/ebucore#",
                            ["mcma"] = "http://mcma.tv#",
                            ["other"] = "http//other#",
                            ["owl"] = "http://www.w3.org/2002/07/owl#",
                            ["rdf"] = "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
                            ["rdfs"] = "http://www.w3.org/2000/01/rdf-schema#",
                            ["xsd"] = "http://www.w3.org/2001/XMLSchema#",

                            // General definition

                            ["id"] = "@id",
                            ["type"] = "@type",

                            ["label"] = "rdfs:label",
                            ["url"] = "xsd:anyURI",

                            // EBUcore definitions

                            ["dateCreated"] = "ebucore:dateCreated",
                            ["dateModified"] = "ebucore:dateModified",

                            // FIMS definitions

                            ["Service"] = "mcma:Service",
                            ["hasResource"] = new JObject
                            {
                                ["@id"] = "mcma:hasServiceResource",
                                ["@type"] = "@id"
                            },

                            ["acceptsJobType"] = new JObject
                            {
                                ["@id"] = "mcma:acceptsJobType",

                                ["@type"] = "@id"
                            },
                            ["acceptsJobProfile"] = new JObject
                            {
                                ["@id"] = "mcma:acceptsJobProfile",
                                ["@type"] = "@id"
                            },

                            ["inputLocation"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobInputLocation",
                                ["@type"] = "@id"
                            },
                            ["outputLocation"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobOutputLocation",
                                ["@type"] = "@id"
                            },

                            ["ServiceResource"] = "mcma:ServiceResource",
                            ["resourceType"] = new JObject
                            {
                                ["@id"] = "mcma:resourceType",
                                ["@type"] = "@id"
                            },

                            ["JobProfile"] = "mcma:JobProfile",
                            ["hasInputParameter"] = new JObject
                            {
                                ["@id"] = "mcma:hasInputParameter",
                                ["@type"] = "@id"
                            },
                            ["hasOptionalInputParameter"] = new JObject
                            {
                                ["@id"] = "mcma:hasOptionalInputParameter",
                                ["@type"] = "@id"
                            },
                            ["hasOutputParameter"] = new JObject
                            {
                                ["@id"] = "mcma:hasOutputParameter",
                                ["@type"] = "@id"
                            },

                            ["JobParameter"] = "mcma:JobParameter",
                            ["jobProperty"] = new JObject
                            {
                                ["@id"] = "mcma:jobProperty",
                                ["@type"] = "@id"
                            },
                            ["parameterType"] = new JObject
                            {
                                ["@id"] = "mcma:jobParameterType",
                                ["@type"] = "@id"
                            },

                            ["Locator"] = "mcma:Locator",
                            ["httpEndpoint"] = new JObject
                            {
                                ["@id"] = "mcma:httpEndpoint",
                                ["@type"] = "xsd:anyURI"
                            },

                            ["awsS3Bucket"] = "mcma:amazonWebServicesS3Bucket",
                            ["awsS3Key"] = "mcma:amazonWebServicesS3Key",
                            ["azureBlobStorageAccount"] = "mcma:microsoftAzureBlobStorageAccount",
                            ["azureBlobStorageContainer"] = "mcma:microsoftAzureBlobStorageContainer",
                            ["azureBlobStorageObjectName"] = "mcma:microsoftAzureBlobStorageObjectName",
                            ["googleCloudStorageBucket"] = "mcma:googleCloudStorageBucket",
                            ["googleCloudStorageObjectName"] = "mcma:googleCloudStorageObjectName",
                            ["uncPath"] = "mcma:uncPath",

                            ["AmeJob"] = "mcma:AmeJob",
                            ["CaptureJob"] = "mcma:CaptureJob",
                            ["QAJob"] = "mcma:QAJob",
                            ["TransformJob"] = "mcma:TransformJob",
                            ["TransferJob"] = "mcma:TransferJob",

                            ["jobProfile"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobProfile",
                                ["@type"] = "@id"
                            },

                            ["jobStatus"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobStatus",
                                ["@type"] = "mcma:JobStatus"
                            },

                            ["jobStatusReason"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobStatusReason",
                                ["@type"] = "xsd:string"
                            },

                            ["jobProcess"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobProcess",
                                ["@type"] = "@id"
                            },

                            ["jobInput"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobInput",
                                ["@type"] = "@id"
                            },

                            ["jobOutput"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobOutput",
                                ["@type"] = "@id"
                            },

                            ["JobParameterBag"] = "mcma:JobParameterBag",

                            ["JobProcess"] = "mcma:JobProcess",

                            ["job"] = new JObject
                            {
                                ["@id"] = "mcma:hasJob",
                                ["@type"] = "@id"
                            },

                            ["jobProcessStatus"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobProcessStatus",
                                ["@type"] = "mcma:JobProcessStatus"
                            },

                            ["jobProcessStatusReason"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobProcessStatusReason",
                                ["@type"] = "xsd:string"
                            },

                            ["jobAssignment"] = new JObject
                            {
                                ["@id"] = "mcma:hasJobAssignment",
                                ["@type"] = "@id"
                            },

                            ["JobAssignment"] = "mcma:JobAssignment",

                            ["asyncEndpoint"] = "mcma:hasAsyncEndpoint",
                            ["AsyncEndpoint"] = "mcma:AsyncEndpoint",

                            ["asyncSuccess"] = new JObject
                            {
                                ["@id"] = "mcma:asyncEndpointSuccess",
                                ["@type"] = "xsd:anyURI"
                            },
                            ["asyncFailure"] = new JObject
                            {
                                ["@id"] = "mcma:asyncEndpointFailure",
                                ["@type"] = "xsd:anyURI"
                            },

                            // Default namespace for custom attributes

                            ["@vocab"] = "http://other#"
                        }
                    });
        }
    }
}