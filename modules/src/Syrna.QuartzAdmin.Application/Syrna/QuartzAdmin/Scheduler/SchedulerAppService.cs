using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace Syrna.QuartzAdmin.Scheduler
{
    public class SchedulerAppService : QuartzAdminAppService, ISchedulerAppService
    {
        protected IScheduler Scheduler => LazyServiceProvider.LazyGetRequiredService<IScheduler>();

        /// <summary>
        /// Getting meta data for a Scheduler.
        /// </summary>
        /// <returns>The Scheduler meta data.</returns>
        [HttpGet]
        public async Task<SchedulerDetails> GetSchedulerMetaData()
        {
            try
            {
                var metaData = await Scheduler.GetMetaData().ConfigureAwait(false);
                return new SchedulerDetails(Scheduler, metaData);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not get scheduler metadata", "CantGetSchedulerMetaData", innerException: ex);
            }
        }

        /// <summary>
        /// Starting the Quartz Scheduler Scheduler.
        /// </summary>
        /// <returns>The Status of the operation.</returns>
        [HttpPost]
        public async Task<ApiResponse> StartScheduler(int? delayMilliseconds = null)
        {
            try
            {
                if (delayMilliseconds == null)
                {
                    await Scheduler.Start().ConfigureAwait(false);
                }
                else
                {
                    await Scheduler.StartDelayed(TimeSpan.FromMilliseconds(delayMilliseconds.Value)).ConfigureAwait(false);
                }

                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not started scheduler", "CantStartScheduler", innerException: ex);
            }
        }

        /// <summary>
        /// Pausing the Quartz Scheduler Scheduler.
        /// </summary>
        /// <returns>The Status of the operation.</returns>
        [HttpGet]
        public async Task<ApiResponse> PauseScheduler()
        {
            try
            {
                await Scheduler.Standby();
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not paused scheduler", "CantPauseScheduler", innerException: ex);
            }
        }

        /// <summary>
        /// Clears all Jobs and Triggers from the Scheduler job store.
        /// </summary>
        /// <returns>The Status of the operation.</returns>
        [HttpDelete]
        public async Task<ApiResponse> ClearScheduler()
        {
            try
            {
                await Scheduler.Clear().ConfigureAwait(false);
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not clear scheduler", "CantClearScheduler", innerException: ex);
            }
        }

        /// <summary>
        /// Shutting down the Quartz Scheduler Scheduler.
        /// </summary>
        /// <param name="waitForJobsToComplete">If set to true the scheduler will let running jobs complete before shutting down.</param>
        /// <returns>The Status of the operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<ApiResponse> ShutDownScheduler(bool waitForJobsToComplete = false)
        {
            try
            {
                await Scheduler.Shutdown(waitForJobsToComplete).ConfigureAwait(false);
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not shutdown scheduler", "CantShutdownScheduler", innerException: ex);
            }
        }

        /// <summary>
        /// Getting a list of <see cref="ExecutingJobContext"/> objects representing all currently executing Jobs in the Scheduler.
        /// </summary>
        /// <returns>The Status of the operation.</returns>
        /// <response code="200">The list of <see cref="ExecutingJobContext"/> objects.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<List<ExecutingJobContext>> GetCurrentExecutingJobs()
        {
            try
            {
                await Scheduler.Clear().ConfigureAwait(false);
                var metaData = await Scheduler.GetCurrentlyExecutingJobs();
                var model = metaData
                    .Select(context => new ExecutingJobContext(context))
                    .ToList();

                return model;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not get current executing jobs", "CantGetCurrentExecutingJobs", innerException: ex);
            }
        }

        /// <summary>
        /// Pausing all jobs in group.
        /// </summary>
        /// <param name="groupName">Job Group Name.</param>
        /// <returns>The Status of the operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        public async Task<ApiResponse> PauseAllJobsInGroup(string groupName)
        {
            try
            {
                await Scheduler.PauseJobs(GroupMatcher<JobKey>.GroupEquals(groupName));
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not pause all jobs in group", "CantPauseAllJobsInGroup", innerException: ex);
            }
        }

        /// <summary>
        /// Resuming all jobs in group.
        /// </summary>
        /// <param name="groupName">Job Group Name.</param>
        /// <returns>The Status of the operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        public async Task<ApiResponse> ResumeAllJobsInGroup(string groupName)
        {
            try
            {
                await Scheduler.ResumeJobs(GroupMatcher<JobKey>.GroupEquals(groupName));
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not resume all jobs in group", "CantResumeAllJobsInGroup", innerException: ex);
            }
        }

        /// <summary>
        /// Pausing all triggers in group.
        /// </summary>
        /// <param name="groupName">Trigger Group Name.</param>
        /// <returns>Status of operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        public async Task<ApiResponse> PauseAllTriggersInGroup(string groupName)
        {
            try
            {
                await Scheduler.PauseTriggers(GroupMatcher<TriggerKey>.GroupEquals(groupName));
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "PauseAllTriggersInGroup");
                throw new UserFriendlyException("Can not pause all triggers in group", "CantPauseAllTriggersInGroup", innerException: ex);
            }

        }

        /// <summary>
        /// Resuming all triggers in group.
        /// </summary>
        /// <param name="groupName">Trigger Group Name.</param>
        /// <returns>Status of operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        public async Task<ApiResponse> ResumeAllTriggersInGroup(string groupName)
        {
            try
            {
                await Scheduler.ResumeTriggers(GroupMatcher<TriggerKey>.GroupEquals(groupName));
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ResumeAllTriggersInGroup");
                throw new UserFriendlyException("Can not resume all triggers in group", "CantResumeAllTriggersInGroup", innerException: ex);
            }

        }

        /// <summary>
        /// Pausing all triggers in scheduler.
        /// </summary>
        /// <returns>Status of the operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<ApiResponse> PauseAll()
        {
            try
            {
                await Scheduler.PauseAll();
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "PauseAll");
                throw new UserFriendlyException("Can not pause all triggers in scheduler", "CantPauseAll", innerException: ex);
            }
        }

        /// <summary>
        /// Resuming all triggers in scheduler.
        /// </summary>
        /// <returns>Status of the operation.</returns>
        /// <response code="204">Ok.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        public async Task<ApiResponse> ResumeAll()
        {
            try
            {
                await Scheduler.ResumeAll();
                return ApiResponse.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ResumeAll");
                throw new UserFriendlyException("Can not resume all triggers in scheduler", "CantResumeAll", innerException: ex);
            }
        }
    }
}
