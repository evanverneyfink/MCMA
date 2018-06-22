using System.Threading.Tasks;

namespace Mcma.Services.Ame.MediaInfo
{
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs a process with the given args
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<ProcessOutput> RunProcess(string path, params string[] args);
    }
}