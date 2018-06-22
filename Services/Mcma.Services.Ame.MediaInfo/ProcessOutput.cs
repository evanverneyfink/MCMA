namespace Mcma.Services.Ame.MediaInfo
{
    public class ProcessOutput
    {
        /// <summary>
        /// Instantiates a <see cref="ProcessOutput"/>
        /// </summary>
        /// <param name="stdOut"></param>
        /// <param name="stdErr"></param>
        public ProcessOutput(string stdOut, string stdErr)
        {
            StdOut = stdOut;
            StdErr = stdErr;
        }

        /// <summary>
        /// Gets the text written to stdout of the process
        /// </summary>
        public string StdOut { get; }

        /// <summary>
        /// Gets the text written to stderr of the process
        /// </summary>
        public string StdErr { get; }
    }
}