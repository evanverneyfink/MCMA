using System;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Core.Serialization;
using Mcma.Server;
using Mcma.Server.Data;
using Mcma.Server.Files;
using Mcma.Services.Jobs.WorkerFunctions;

namespace Mcma.Services.Ame.MediaInfo
{
    public class MediaInfoWorker : Worker<AmeJob>
    {
        #region Constructors

        /// <summary>
        /// Instantiates a <see cref="MediaInfoWorker"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dataHandler"></param>
        /// <param name="accessibleLocationProvider"></param>
        /// <param name="processRunner"></param>
        /// <param name="mediaInfoOutputConverter"></param>
        /// <param name="fileStorage"></param>
        /// <param name="resourceSerializer"></param>
        /// <param name="processLocator"></param>
        public MediaInfoWorker(ILogger logger,
                               IResourceDataHandler dataHandler,
                               IMediaInfoAccessibleLocationProvider accessibleLocationProvider,
                               IProcessRunner processRunner,
                               IMediaInfoOutputConverter mediaInfoOutputConverter,
                               IFileStorage fileStorage,
                               IResourceSerializer resourceSerializer,
                               IMediaInfoProcessLocator processLocator)
            : base(dataHandler)
        {
            Logger = logger;
            AccessibleLocationProvider = accessibleLocationProvider;
            ProcessRunner = processRunner;
            MediaInfoOutputConverter = mediaInfoOutputConverter;
            FileStorage = fileStorage;
            ResourceSerializer = resourceSerializer;
            ProcessLocator = processLocator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the accessible url provider
        /// </summary>
        private IMediaInfoAccessibleLocationProvider AccessibleLocationProvider { get; }

        /// <summary>
        /// Gets the process runner
        /// </summary>
        private IProcessRunner ProcessRunner { get; }

        /// <summary>
        /// Gets the MediaInfo output converter
        /// </summary>
        private IMediaInfoOutputConverter MediaInfoOutputConverter { get; }

        /// <summary>
        /// Gets file storage
        /// </summary>
        private IFileStorage FileStorage { get; }

        /// <summary>
        /// Gets the resource serializer
        /// </summary>
        private IResourceSerializer ResourceSerializer { get; }

        /// <summary>
        /// Gets the process locator
        /// </summary>
        private IMediaInfoProcessLocator ProcessLocator { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Runs media info and stores the output to a file
        /// </summary>
        /// <returns></returns>
        public override async Task Execute(AmeJob job)
        {
            Logger.Info("Starting MediaInfo job {0}...", job?.Id);

            try
            {
                if (job == null) throw new ArgumentNullException(nameof(job));

                Logger.Debug("Job JSON: {0}", ResourceSerializer.Serialize(job));

                // ensure this is a valid profile for this worker
                if (job.JobProfile.Label != "ExtractTechnicalMetadata")
                    throw new Exception($"JobProfile '{job.JobProfile.Label}' not accepted");

                // ensure the job specifies input and output locations
                if (job.JobInput["inputFile"] == null)
                    throw new Exception("Job does not specify an input location.");
                // ensure the job specifies an output location
                if (job.JobInput["outputLocation"] == null)
                    throw new Exception("Job does not specify an output location.");

                var serializedOutputLocation = job.JobInput["outputLocation"]?.ToString();
                if (serializedOutputLocation == null)
                    throw new Exception("Failed to resolve jobInput[\"outputLocation\"]");

                Logger.Debug("Deserializing output location from {0}...", serializedOutputLocation);

                // get output locator
                var outputLocation = await ResourceSerializer.Deserialize<Locator>(serializedOutputLocation);

                var serializedInputFile = job.JobInput["inputFile"]?.ToString();
                if (serializedInputFile == null)
                    throw new Exception("Failed to resolve jobInput[\"inputFile\"]");

                Logger.Debug("Deserializing input file path from {0}...", serializedInputFile);

                // get input locator
                var inputFile = await ResourceSerializer.Deserialize<Locator>(serializedInputFile);
                
                Logger.Debug("Getting url for input file that's accessible by MediaInfo...");

                // get the url of the file MediaInfo should use (could be local, S3, etc)
                var accessibleUrl = await AccessibleLocationProvider.GetMediaInfoAccessibleLocation(inputFile);
                if (accessibleUrl == null)
                    throw new Exception("Input file is not accessible to MediaInfo.");

                Logger.Debug("Getting location of MediaInfo binary...");

                var mediaInfoLocation = ProcessLocator.GetMediaInfoLocation();
                
                Logger.Debug("Running MediaInfo from {0} against input file {1}...", mediaInfoLocation, accessibleUrl);

                // run the media info process
                var result = await ProcessRunner.RunProcess(mediaInfoLocation, "--Output=EBUCore", accessibleUrl);
                if (!string.IsNullOrWhiteSpace(result.StdErr))
                    throw new Exception($"MediaInfo returned one or more errors: {result.StdErr}.");

                Logger.Debug("MediaInfo successfully ran against input file {0}. Converting output to JSON...", accessibleUrl);

                // convert output to JSON
                var mediaInfoJson = MediaInfoOutputConverter.GetJson(result.StdOut);

                Logger.Debug("Storing MediaInfo JSON for {0} to file...", accessibleUrl);

                // save JSON to file and store the resulting locator in the job output
                var outputLocator = await FileStorage.WriteTextToFile(outputLocation, Guid.NewGuid().ToString(), mediaInfoJson);
                job.JobOutput["outputFile"] = outputLocator;
                
                Logger.Debug("MediaInfo JSON stored to file. Locator = {0}", ResourceSerializer.Serialize(outputLocator));
                Logger.Info("MediaInfo job {0} completed successfully.", job.Id);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred running the MediaInfo job. Exception: {0}", ex);
                throw;
            }
        }

        #endregion
    }
}
