using JsonLD.Core;
using Newtonsoft.Json.Linq;

namespace Mcma.Services.Ame.MediaInfo
{
    public static class MediaInfoContexts
    {
        public static readonly Context Default =
            new Context(
                new JObject
                {
                    ["dc"] = "http://purl.org/dc/elements/1.1/",
                    ["ebucore"] = "http://www.ebu.ch/metadata/ontologies/ebucore/ebucore#",
                    ["mcma"] = "http://mcma.tv#",
                    ["mediaInfo"] = "https://mediaarea.net#",
                    ["rdf"] = "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
                    ["rdfs"] = "http://www.w3.org/2000/01/rdf-schema#",
                    ["xsd"] = "http://www.w3.org/2001/XMLSchema#",
                    ["xsi"] = "http://www.w3.org/2001/XMLSchema-instance",

                    ["ebucore:hasVideoFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:hasVideoEncodingFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:frameWidth"] = new JObject
                    {
                        ["@id"] = "ebucore:frameWidth",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:frameHeight"] = new JObject
                    {
                        ["@id"] = "ebucore:frameHeight",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:frameRate"] = new JObject
                    {
                        ["@id"] = "ebucore:frameRate",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasVideoEncodingFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:hasVideoEncodingFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasVideoCodec"] = new JObject
                    {
                        ["@id"] = "ebucore:hasVideoCodec",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:videoBitRate"] = new JObject
                    {
                        ["@id"] = "ebucore:videoBitRate",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:videoBitRateMax"] = new JObject
                    {
                        ["@id"] = "ebucore:videoBitRateMax",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:videoBitRateMode"] = new JObject
                    {
                        ["@id"] = "ebucore:videoBitRateMode",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:scanningFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:scanningFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasVideoTrack"] = new JObject
                    {
                        ["@id"] = "ebucore:hasVideoTrack",
                        ["@type"] = "@id"
                    },
                    ["ebucore:trackNumber"] = new JObject
                    {
                        ["@id"] = "ebucore:trackNumber",
                        ["@type"] = "xsd:integer"
                    },
                    ["mediaInfo:Standard"] = new JObject
                    {
                        ["@id"] = "mediaInfo:Standard",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:ColorSpace"] = new JObject
                    {
                        ["@id"] = "mediaInfo:ColorSpace",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:colour_primaries"] = new JObject
                    {
                        ["@id"] = "mediaInfo:colour_primaries",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:transfer_characteristics"] = new JObject
                    {
                        ["@id"] = "mediaInfo:transfer_characteristics",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:matrix_coefficients"] = new JObject
                    {
                        ["@id"] = "mediaInfo:matrix_coefficients",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:colour_range"] = new JObject
                    {
                        ["@id"] = "mediaInfo:colour_range",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:VideoStreamSize"] = new JObject
                    {
                        ["@id"] = "mediaInfo:VideoStreamSize",
                        ["@type"] = "xsd:integer"
                    },
                    ["mediaInfo:BitDepth"] = new JObject
                    {
                        ["@id"] = "mediaInfo:BitDepth",
                        ["@type"] = "xsd:integer"
                    },
                    ["mediaInfo:CABAC"] = new JObject
                    {
                        ["@id"] = "mediaInfo:CABAC",
                        ["@type"] = "xsd:boolean"
                    },
                    ["mediaInfo:MBAFF"] = new JObject
                    {
                        ["@id"] = "mediaInfo:MBAFF",
                        ["@type"] = "xsd:boolean"
                    },

                    ["ebucore:hasAudioFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:hasAudioFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasAudioEncodingFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:hasAudioEncodingFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasAudioCodec"] = new JObject
                    {
                        ["@id"] = "ebucore:hasAudioCodec",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:sampleRate"] = new JObject
                    {
                        ["@id"] = "ebucore:sampleRate",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:audioBitRate"] = new JObject
                    {
                        ["@id"] = "ebucore:audioBitRate",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:audioBitRateMax"] = new JObject
                    {
                        ["@id"] = "ebucore:audioBitRateMax",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:audioBitRateMode"] = new JObject
                    {
                        ["@id"] = "ebucore:audioBitRateMode",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasAudioTrack"] = new JObject
                    {
                        ["@id"] = "ebucore:hasAudioTrack",
                        ["@type"] = "@id"
                    },
                    ["ebucore:trackId"] = new JObject
                    {
                        ["@id"] = "ebucore:trackId",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:hasLanguage"] = new JObject
                    {
                        ["@id"] = "ebucore:hasLanguage",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:audioChannelNumber"] = new JObject
                    {
                        ["@id"] = "ebucore:audioChannelNumber",
                        ["@type"] = "xsd:integer"
                    },
                    ["mediaInfo:ChannelPositions"] = new JObject
                    {
                        ["@id"] = "mediaInfo:ChannelPositions",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:ChannelLayout"] = new JObject
                    {
                        ["@id"] = "mediaInfo:ChannelLayout",
                        ["@type"] = "xsd:string"
                    },
                    ["mediaInfo:AudioStreamSize"] = new JObject
                    {
                        ["@id"] = "mediaInfo:AudioStreamSize",
                        ["@type"] = "xsd:integer"
                    },

                    ["ebucore:hasContainerFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:hasContainerFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasContainerEncodingFormat"] = new JObject
                    {
                        ["@id"] = "ebucore:hasContainerEncodingFormat",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:hasContainerCodec"] = new JObject
                    {
                        ["@id"] = "ebucore:hasContainerCodec",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:durationNormalPlayTime"] = new JObject
                    {
                        ["@id"] = "ebucore:durationNormalPlayTime",
                        ["@type"] = "xsd:duration"
                    },
                    ["ebucore:fileSize"] = new JObject
                    {
                        ["@id"] = "ebucore:fileSize",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:filename"] = new JObject
                    {
                        ["@id"] = "ebucore:filename",
                        ["@type"] = "xsd:string"
                    },
                    ["ebucore:bitRateOverall"] = new JObject
                    {
                        ["@id"] = "ebucore:bitRateOverall",
                        ["@type"] = "xsd:integer"
                    },
                    ["ebucore:dateCreated"] = new JObject
                    {
                        ["@id"] = "ebucore:dateCreated",
                        ["@type"] = "xsd:dateTime"
                    },
                    ["ebucore:dateModified"] = new JObject
                    {
                        ["@id"] = "ebucore:dateModified",
                        ["@type"] = "xsd:dateTime"
                    },
                });
    }
}