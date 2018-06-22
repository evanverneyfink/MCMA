using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mcma.Services.Ame.MediaInfo
{
    internal class MediaInfoOutputConverter : IMediaInfoOutputConverter
    {
        /// <summary>
        /// Converts MediaInfo output to JSON
        /// </summary>
        /// <param name="stdOut"></param>
        /// <returns></returns>
        public string GetJson(string stdOut)
        {
            // get xml
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(stdOut);

            // generate json from xml
            var fromXml = JObject.Parse(JsonConvert.SerializeXmlNode(xmlDoc));

            // convert the generated json to output json
            var convertedOutput = GenerateOutput(fromXml);

            // return the output JSON
            return convertedOutput.ToString();
        }

        /// <summary>
        /// Extracts metadata from a path in the JSON object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private JToken ExtractMetadata(JToken obj, string path, JToken defaultValue = null)
        {
            foreach (var part in path.Split('/'))
            {
                // check if this part is an integer
                var isInt = int.TryParse(part, out var partInt);

                // if this is a single value but we are expecting an array, return the single value
                if (obj is JValue && isInt && partInt == 0)
                    continue;

                JToken next = null;

                // try to get the next object
                if (obj is JArray jArray && isInt && partInt < jArray.Count)
                    next = obj[partInt];
                else if (obj is JObject)
                {
                    // if the path is for an array with just 1 element in it, the XML-to-JSON
                    // conversion will convert it to a single object rather an array, so we
                    // can skip over the array indexer and just try for the object instead
                    if (isInt && partInt == 0)
                        continue;

                    next = obj[part];
                }

                // nothing there - return the default value
                if (next == null)
                    return defaultValue;

                obj = next;
            }

            return obj;
        }

        /// <summary>
        /// Generates output JSON from the JSON converted from xml
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private JObject GenerateOutput(JObject input)
        {
            var output = new JObject
            {
                ["@context"] = MediaInfoContexts.Default,
                ["@type"] = "ebucore:BMEssence",
                ["ebucore:hasVideoFormat"] = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/@videoFormatName"),
                ["ebucore:frameWidth"] = ExtractMetadata(input,
                                                         "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:width/0/#text"),
                ["ebucore:frameHeight"] = ExtractMetadata(input,
                                                          "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:height/0/#text"),
                ["ebucore:frameRate"] = ExtractMetadata(input,
                                                        "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:frameRate/0/@factorNumerator") +
                                        "/" + ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:frameRate/0/@factorDenominator"),
                ["ebucore:displayAspectRatio"] = ExtractMetadata(input,
                                                                 "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:aspectRatio/0/ebucore:factorNumerator") +
                                                 ":" + ExtractMetadata(input,
                                                                       "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:aspectRatio/0/ebucore:factorDenominator"),
                ["ebucore:hasVideoEncodingFormat"] = ExtractMetadata(input,
                                                                     "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:videoEncoding/0/@typeLabel"),
                ["ebucore:hasVideoCodec"] = ExtractMetadata(input,
                                                            "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:codec/0/ebucore:codecIdentifier/0/dc:identifier/0"),
                ["ebucore:videoBitRate"] = ExtractMetadata(input,
                                                           "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:bitRate/0"),
                ["ebucore:videoBitRateMax"] = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:bitRateMax/0"),
                ["ebucore:videoBitRateMode"] = ExtractMetadata(input,
                                                               "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:bitRateMode/0"),
                ["ebucore:scanningFormat"] = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:scanningFormat/0"),
                ["ebucore:hasVideoTrack"] = new JObject
                {
                    ["@type"] = "ebucore:VideoTrack",
                    ["ebucore:trackNumber"] = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:videoTrack/0/@trackId")
                }
            };


            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:technicalAttributeString/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:technicalAttributeString/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "Standard":
                    case "ColorSpace":
                    case "ChromaSubSampling":
                    case "colour_primaries":
                    case "transfer_characteristics":
                    case "matrix_coefficients":
                    case "colour_range":
                        output["mediaInfo:" + technicalAttributeName] = technicalAttributeValue;
                        break;
                }
            }

            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:technicalAttributeInteger/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:technicalAttributeInteger/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "StreamSize":
                        output["mediaInfo:Video" + technicalAttributeName] = technicalAttributeValue;
                        break;
                    case "BitDepth":
                        output["mediaInfo:" + technicalAttributeName] = technicalAttributeValue;
                        break;
                }
            }

            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:technicalAttributeBoolean/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:videoFormat/0/ebucore:technicalAttributeBoolean/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "CABAC":
                    case "MBAFF":
                        output["mediaInfo:" + technicalAttributeName] = technicalAttributeValue;
                        break;
                }
            }

            output["ebucore:hasAudioFormat"] = ExtractMetadata(input,
                                                               "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/@audioFormatName");
            output["ebucore:hasAudioEncodingFormat"] = ExtractMetadata(input,
                                                                       "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:audioEncoding/0/@typeLabel");
            output["ebucore:hasAudioCodec"] = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:codec/0/ebucore:codecIdentifier/0/dc:identifier/0");
            output["ebucore:sampleRate"] = ExtractMetadata(input,
                                                           "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:samplingRate/0");
            output["ebucore:audioBitRate"] = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:bitRate/0");
            output["ebucore:audioBitRateMax"] = ExtractMetadata(input,
                                                                "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:bitRateMax/0");
            output["ebucore:audioBitRateMode"] = ExtractMetadata(input,
                                                                 "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:bitRateMode/0");
            output["ebucore:hasAudioTrack"] = new JObject
            {
                ["@type"] = "ebucore:AudioTrack",
                ["trackId"] = ExtractMetadata(input,
                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:audioTrack/0/@trackId"),
                ["hasLanguage"] = ExtractMetadata(input,
                                                  "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:audioTrack/0/@trackLanguage")
            };
            output["ebucore:audioChannelNumber"] = ExtractMetadata(input,
                                                                   "ebucore:ebuCoreMain/0/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:channels/0");

            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:technicalAttributeString/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:technicalAttributeString/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "ChannelPositions":
                    case "ChannelLayout":
                        output["mediaInfo:" + technicalAttributeName] = technicalAttributeValue;
                        break;
                }
            }

            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:technicalAttributeInteger/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:audioFormat/0/ebucore:technicalAttributeInteger/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "StreamSize":
                        output["mediaInfo:Audio" + technicalAttributeName] = technicalAttributeValue;
                        break;
                }
            }

            output["ebucore:hasContainerFormat"] = ExtractMetadata(input,
                                                                   "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:containerFormat/0/@containerFormatName");

            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:containerFormat/0/ebucore:technicalAttributeString/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:containerFormat/0/ebucore:technicalAttributeString/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "FormatProfile":
                        output["ebucore:hasContainerEncodingFormat"] = technicalAttributeValue;
                        break;
                }
            }

            output["ebucore:hasContainerCodec"] = ExtractMetadata(input,
                                                                  "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:containerFormat/0/ebucore:codec/0/ebucore:codecIdentifier/0/dc:identifier/0");

            output["ebucore:durationNormalPlayTime"] = ExtractMetadata(input,
                                                                       "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:duration/0/ebucore:normalPlayTime/0");
            output["ebucore:fileSize"] = ExtractMetadata(input, "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:fileSize/0");
            output["ebucore:fileName"] = ExtractMetadata(input, "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:fileName/0");

            for (var i = 0;; i++)
            {
                var technicalAttributeValue = ExtractMetadata(input,
                                                              "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:technicalAttributeInteger/" +
                                                              i + "/#text");
                var technicalAttributeName = ExtractMetadata(input,
                                                             "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:technicalAttributeInteger/" +
                                                             i + "/@typeLabel");
                if (technicalAttributeValue == null || technicalAttributeName == null)
                {
                    break;
                }

                switch (technicalAttributeName.Value<string>())
                {
                    case "OverallBitRate":
                        output["ebucore:bitRateOverall"] = technicalAttributeValue;
                        break;
                }
            }


            output["ebucore:dateCreated"] =
                ExtractMetadata(input, "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:dateCreated/0/@startDate") + "T" +
                ExtractMetadata(input, "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:dateCreated/0/@startTime");
            output["ebucore:dateModified"] =
                ExtractMetadata(input, "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:dateModified/0/@startDate") + "T" +
                ExtractMetadata(input, "ebucore:ebuCoreMain/ebucore:coreMetadata/0/ebucore:format/0/ebucore:dateModified/0/@startTime");

            return output;
        }
    }
}