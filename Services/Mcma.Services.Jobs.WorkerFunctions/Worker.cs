using System;
using System.Threading.Tasks;
using Mcma.Core.Model;
using Mcma.Server.Data;

namespace Mcma.Services.Jobs.WorkerFunctions
{
    public abstract class Worker<T> : IWorker where T : Job, new()
    {
        /// <summary>
        /// Instantiates a <see cref="Worker{T}"/>
        /// </summary>
        /// <param name="dataHandler"></param>
        protected Worker(IResourceDataHandler dataHandler)
        {
            DataHandler = dataHandler;
        }

        /// <summary>
        /// Gets the resource data handler
        /// </summary>
        protected IResourceDataHandler DataHandler { get; }

        /// <summary>
        /// Executes work for the given input
        /// </summary>
        /// <param name="jobAssignment"></param>
        /// <returns></returns>
        async Task IWorker.Execute(JobAssignment jobAssignment)
        {
            JobProcess jobProcess = null;
            T job = null;
            try
            {
                // update status and modified date/time
                jobAssignment.JobProcessStatus = "Running";
                jobAssignment.DateModified = DateTime.UtcNow;

                // persist the updated job assignment
                await DataHandler.Update(jobAssignment);

                // get the job process from the job assignment
                jobProcess = await DataHandler.Get<JobProcess>(jobAssignment.JobProcess);
                if (jobProcess == null)
                    throw new Exception("Failed to resolve jobAssignment.jobProcess");

                // get the job from the job process
                job = await DataHandler.Get<T>(jobProcess.Job.Id);
                if (job == null)
                    throw new Exception("Failed to resolve jobProcess.job");

                // load the job profile
                job.JobProfile = await DataHandler.Get<JobProfile>(job.JobProfile.Id);
                if (job.JobProfile == null)
                    throw new Exception("Failed to resolve job.jobProfile");

                // do the work
                await Execute(job);

                // update the job with output
                job.JobStatus = jobAssignment.JobProcessStatus = "Completed";
                await DataHandler.Update(job);

                // mark the job completed and set its output
                jobAssignment.JobOutput = job.JobOutput;
            }
            catch (Exception exception)
            {
                // mark failed if an exception is encountered
                jobAssignment.JobProcessStatus = "Failed";
                jobAssignment.JobProcessStatusReason = exception.ToString();
            }

            // update the job assignment to current state after attempting to run the job
            await DataHandler.Update(jobAssignment);

            // update the job process from the job assignment
            if (jobProcess != null)
            {
                jobProcess.JobProcessStatus = jobAssignment.JobProcessStatus;
                jobProcess.JobProcessStatusReason = jobAssignment.JobProcessStatusReason;
                jobProcess.JobEnd = DateTime.UtcNow;

                await DataHandler.Update(jobProcess);
            }

            // hit the async callback endpoint
            if (job?.AsyncEndpoint != null)
            {
                switch (jobAssignment.JobProcessStatus)
                {
                    case "Failed" when !string.IsNullOrWhiteSpace(job.AsyncEndpoint.AsyncFailure):
                        await DataHandler.Get<Resource>(job.AsyncEndpoint.AsyncFailure);
                        break;
                    case "Completed" when !string.IsNullOrWhiteSpace(job.AsyncEndpoint.AsyncSuccess):
                        await DataHandler.Get<Resource>(job.AsyncEndpoint.AsyncSuccess);
                        break;
                }
            }
        }

        /// <summary>
        /// Executes work the given input
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public abstract Task Execute(T job);
    }
}