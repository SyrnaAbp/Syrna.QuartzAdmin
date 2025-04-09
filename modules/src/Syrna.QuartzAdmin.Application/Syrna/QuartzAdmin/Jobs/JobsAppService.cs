using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace Syrna.QuartzAdmin.Jobs
{
    public class JobsAppService : QuartzAdminAppService, IJobsAppService
    {
        protected IScheduler Scheduler => LazyServiceProvider.LazyGetRequiredService<IScheduler>();

        /// <summary>
        /// Getting a list of <see cref="JobListDetail"/> for all configured <see cref="IJob"/> instances in the <see cref="IScheduler"/>.
        /// </summary>
        /// <returns>The list of configured jobs..</returns>
        /// <response code="200">Returns the list of configured jobs for the scheduler..</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<List<JobListDetail>> GetAllJobs()
        {
            try
            {
                var keys = (await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
                    .OrderBy(x => x.ToString());

                var jobList = new List<JobListDetail>();
                foreach (var key in keys)
                {
                    var triggers = await Scheduler.GetTriggersOfJob(key);
                    var nextFires = triggers.Select(x => x.GetNextFireTimeUtc()?.UtcDateTime).ToArray();
                    var lastFires = triggers.Select(x => x.GetPreviousFireTimeUtc()?.UtcDateTime).ToArray();
                    var detail = await Scheduler.GetJobDetail(key);
                    var item = new JobListDetail()
                    {
                        ConcurrentExecutionDissallowed = !detail.ConcurrentExecutionDisallowed,
                        PersistJobDataAfterExecution = detail.PersistJobDataAfterExecution,
                        RequestRecovery = detail.RequestsRecovery,
                        Durable = detail.Durable,
                        Name = key.Name,
                        Group = key.Group,
                        JobType = detail.JobType.FullName,
                        Description = detail.Description,
                        NextFireTimeUtc = nextFires.Where(x => x != null).OrderBy(x => x).FirstOrDefault(),
                        LastFireTimeUtc = lastFires.Where(x => x != null).OrderBy(x => x).LastOrDefault()
                    };
                    jobList.Add(item);
                }

                return jobList;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not get all jobs", "CantGetAllJobs", innerException: ex);
            }
        }

        /// <summary>
        /// Interrupts the execution of a job.
        /// </summary>
        /// <param name="fireInstanceId">The id of the running job instance.</param>
        /// <returns>True if the job is interrupted, otherwise false.</returns>
        /// <response code="200">True if job was interrupted.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        public async Task<bool> InterruptJob(string fireInstanceId)
        {
            try
            {
                var result = await Scheduler.Interrupt(fireInstanceId);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "InterruptJob");
                throw new UserFriendlyException("Can not interrupt job", "CantInterruptJob", innerException: ex);
            }
        }

        /// <summary>
        /// Deleting job ín scheduler.
        /// </summary>
        /// <param name="jobGroup">Job Group Name.</param>
        /// <param name="jobName">Job Name.</param>
        /// <returns>Status of the operation.</returns>
        /// <response code="200">True if the job was deleted.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpDelete]
        public async Task<bool> DeleteJob(string jobGroup, string jobName)
        {
            try
            {
                var result = await Scheduler.DeleteJob(new JobKey(jobName, jobGroup));
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "TriggerJob Name: {jobName} Group: {jobGroup}", jobName, jobGroup);
                throw new UserFriendlyException("Can not delete job", "CantDeleteJob", innerException: ex);
            }
        }

        /// <summary>
        /// Triggering job ín scheduler.
        /// </summary>
        /// <param name="jobGroup">Job Group Name.</param>
        /// <param name="jobName">Job Name.</param>
        /// <returns>Status of the operation.</returns>
        /// <response code="204">Success.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<ApiResponse> TriggerJob(string jobGroup, string jobName)
        {
            try
            {
                await Scheduler.TriggerJob(new JobKey(jobName, jobGroup));
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "TriggerJob Name: {jobName} Group: {jobGroup}", jobName, jobGroup);
                throw new UserFriendlyException("Can not trigger job", "CantTriggerJob", innerException: ex);
            }
        }

        /// <summary>
        /// Getting job details from scheduler.
        /// </summary>
        /// <param name="jobGroup">Job Group Name.</param>
        /// <param name="jobName">Job Name.</param>
        /// <returns>The <see cref="IJobDetail"/>.</returns>
        /// <response code="200">Success.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<IJobDetail> GetJobConfiguration(string jobGroup, string jobName)
        {
            try
            {
                if (string.IsNullOrEmpty(jobGroup) || string.IsNullOrEmpty(jobName))
                    throw new UserFriendlyException("JobName or JobGroup is invalid", "InvalidJobNameOrJobGroup");

                var jobDetails = await Scheduler.GetJobDetail(new JobKey(jobName, jobGroup));

                return jobDetails;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EditJobConfiguration");
                throw new UserFriendlyException("Can not get job configuration", "CantGetJobConfiguration", innerException: ex);
            }
        }

        /// <summary>
        /// Creating a new <see cref="IJob"/>.
        /// </summary>
        /// <param name="model">The Job details.</param>
        /// <returns></returns>
        /// <response code="204">Success.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        public async Task<ApiResponse> CreateJobConfiguration(JobDetails model)
        {
            return await AddEditJobDetails(model, false);
        }

        /// <summary>
        /// Updating a <see cref="IJob"/>.
        /// </summary>
        /// <param name="model">The Job details.</param>
        /// <returns></returns>
        /// <response code="204">Success.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPut]
        public async Task<ApiResponse> UpdateJobConfiguration(JobDetails model)
        {
            return await AddEditJobDetails(model, true);
        }

        private async Task<ApiResponse> AddEditJobDetails(JobDetails model, bool replace)
        {
            try
            {
                var jobDetail = new JobDetailImpl(model.Name,
                    model.Group,
                    Type.GetType(model.JobType),
                    model.Durable,
                    model.RequestsRecovery)
                {
                    Description = model.Description
                };
                jobDetail.Validate();
                await Scheduler.AddJob(jobDetail, replace).ConfigureAwait(false);
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AddEditJobDetails");
                throw new UserFriendlyException("Can not add or edit job details", "CantAddEditJobDetails", innerException: ex);
            }
        }
    }
}
