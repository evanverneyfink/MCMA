using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Mcma.Core;

namespace Mcma.Services.Ame.MediaInfo
{
    internal class ProcessRunner : IProcessRunner
    {
        /// <summary>
        /// Runs a process with the given args
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Task<ProcessOutput> RunProcess(string path, params string[] args)
        {
            using (var processInstance = new ProcessInstance(path, args))
            {
                processInstance.RunProcess();

                return Task.FromResult(processInstance.Output);
            }
        }

        private class ProcessInstance : IDisposable
        {
            /// <summary>
            /// Instantiates a <see cref="ProcessInstance"/>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="args"></param>
            public ProcessInstance(string path, params string[] args)
            {
                // parameters for running the process
                Process = new Process
                {
                    StartInfo =
                    {
                        FileName = path,
                        Arguments = args?.Join(" ") ?? string.Empty,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                Process.OutputDataReceived += ProcessOnOutputDataReceived;
                Process.ErrorDataReceived += ProcessOnErrorDataReceived;
            }

            /// <summary>
            /// Gets the underlying process
            /// </summary>
            private Process Process { get; }

            /// <summary>
            /// Gets the standard output from the process
            /// </summary>
            private string StandardOutput { get; set; } = string.Empty;

            /// <summary>
            /// Gets the standard error from the process
            /// </summary>
            private string StandardError { get; set; } = string.Empty;

            /// <summary>
            /// Gets the standard output and error wrapped into a <see cref="ProcessOutput"/>
            /// </summary>
            public ProcessOutput Output => new ProcessOutput(StandardOutput, StandardError);

            /// <summary>
            /// Runs the instance of the process
            /// </summary>
            /// <returns></returns>
            public void RunProcess()
            {
                Process.Start();
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();

                // wait for the process to finish
                Process.WaitForExit();
            }

            /// <summary>
            /// Handles data received from stdout
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="dataReceivedEventArgs"></param>
            private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs) => StandardOutput += dataReceivedEventArgs.Data;

            /// <summary>
            /// Handles data received from stderr
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="dataReceivedEventArgs"></param>
            private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs) => StandardError += dataReceivedEventArgs.Data;

            /// <summary>
            /// Disposes of the underlying process
            /// </summary>
            public void Dispose()
            {
                Process.OutputDataReceived -= ProcessOnOutputDataReceived;
                Process.ErrorDataReceived -= ProcessOnErrorDataReceived;
                Process?.Dispose();
            }
        }
    }
}