using System;
using System.Linq;
using Mcma.Core.Model;

namespace Mcma.Core.Jobs
{
    public static class JobValidationExtensions
    {
        /// <summary>
        /// Validates a job before processing
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static void Validate(this Job job)
        {
            // ensure profile is set
            if (job.JobProfile == null)
                throw new Exception("Missing JobProfile");

            if (job.JobProfile.HasInputParameter != null && job.JobProfile.HasInputParameter.Any())
            {
                foreach (var inputParam in job.JobProfile.HasInputParameter)
                {
                    if (inputParam.JobProperty == null)
                        throw new Exception("Invalid JobProfile: inputParameter without 'mcma:jobProperty' detected");
                    if (string.IsNullOrWhiteSpace(inputParam.JobProperty))
                        throw new Exception("Invalid JobProfile: inputParameter with wrongly defined 'mcma:jobProperty' detected");

                    var inputPropertyName = inputParam.JobProperty;

                    if (job.JobInput[inputPropertyName] == null)
                        throw new Exception($"Invalid Job: Missing required input parameter '{inputPropertyName}'");

                    if (inputParam.ParameterType != null &&
                        !inputParam.ParameterType.IsInstanceOfType(job.JobInput[inputPropertyName]))
                        throw new Exception($"Invalid Job: Required input parameter '{inputPropertyName}' has wrong type");
                }
            }
        }

        /// <summary>
        /// Checks if a service can accept a given job
        /// </summary>
        /// <param name="service"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public static bool CanAcceptJob(this Service service, Job job)
        {
            return
                service.AcceptsJobType != null &&
                service.AcceptsJobType.Any(jt => jt.IsInstanceOfType(job)) &&
                service.AcceptsJobProfile != null &&
                service.AcceptsJobProfile.Any(jp => jp.Label == job.JobProfile.Label);
        }
    }
}
