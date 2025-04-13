using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Logging;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                return SchedulerDetails.Create(Scheduler, metaData);
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
        public async Task<ApiResponse> StartSchedulerExt(int? delayMilliseconds = null)
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
        public async Task<ApiResponse> ShutDownSchedulerExt(bool waitForJobsToComplete = false)
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
                    .Select(context => ExecutingJobContext.Create(context))
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

        //CAK
        [HttpGet]
        public async IAsyncEnumerable<ScheduleModel> GetAllJobsAsync(ScheduleJobFilter filter = null)
        {
            var jobGroupNames = await Scheduler.GetJobGroupNames();

            foreach (var jobGrp in jobGroupNames)
            {
                if (filter != null && !filter.IncludeSystemJobs)
                {
                    if (jobGrp == Constants.SYSTEM_GROUP)
                        continue;
                }

                var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGrp));

                foreach (var jobKey in jobKeys)
                {
                    await foreach (var job in GetScheduleModelsAsync(jobKey))
                    {
                        yield return job;
                    }
                }
            }
        }

        [HttpGet]
        public async Task<ScheduleModel> GetScheduleModelAsync(ITrigger trigger)
        {
            var jobDetail = await Scheduler.GetJobDetail(trigger.JobKey);

            return await CreateScheduleModel(jobDetail, trigger);
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<string>> GetJobGroups()
        {
            return (await Scheduler.GetJobGroupNames())
                .Where(n => n != Constants.SYSTEM_GROUP).ToList();
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<string>> GetTriggerGroups()
        {
            return (await Scheduler.GetTriggerGroupNames()).
                Where(n => n != Constants.SYSTEM_GROUP).ToList();
        }

        [HttpGet]
        public async Task<IList<KeyValuePair<string, int>>> GetScheduledJobSummary()
        {
            var executingCount = (await Scheduler.GetCurrentlyExecutingJobs()).Count;
            var jobCount = (await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())).Count;
            var triggerCount = (await Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup())).Count;
            var sysJobCount = (await Scheduler.GetJobKeys(
                GroupMatcher<JobKey>.GroupEquals(Constants.SYSTEM_GROUP))).Count;
            var sysTriggerCount = (await Scheduler.GetTriggerKeys(
                GroupMatcher<TriggerKey>.GroupEquals(Constants.SYSTEM_GROUP))).Count;

            return new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("Jobs", jobCount),
                new KeyValuePair<string, int>("Triggers", triggerCount),
                new KeyValuePair<string, int>("Executing", executingCount),
                new KeyValuePair<string, int>("System Jobs", sysJobCount),
                new KeyValuePair<string, int>("System Triggers", sysTriggerCount)
            };
        }

        [HttpGet]
        public async Task<SchedulerMetaData> GetMetadataAsync()
        {
            return await Scheduler.GetMetaData();
        }

        [HttpPost]
        public async Task CreateSchedule(JobDetailModel jobDetailModel, TriggerDetailModel triggerDetailModel)
        {
            var trigger = BuildTrigger(triggerDetailModel);

            // Determine if job already exists
            if (await ContainsJobKey(jobDetailModel.Name, jobDetailModel.Group))
            {
                var existingJob = await Scheduler.GetJobDetail(new JobKey(jobDetailModel.Name, jobDetailModel.Group));
                if (existingJob != null)
                {
                    //await scheduler.GetTriggersOfJob(job.Key)
                    var jobTriggers = new List<ITrigger>(1);
                    jobTriggers.Add(trigger);

                    await Scheduler.ScheduleJob(existingJob, jobTriggers.AsReadOnly(), true);
                    return;
                }
            }

            var job = CreateJobDetail(jobDetailModel);

            await Scheduler.ScheduleJob(job, trigger);
        }

        [HttpPost]
        public async Task UpdateSchedule(Key oldJobKey, Key oldTriggerKey, JobDetailModel newJobModel, TriggerDetailModel newTriggerModel)
        {
            var oJobKey = oldJobKey.ToJobKey();

            var newJob = CreateJobDetail(newJobModel);
            var trigger = BuildTrigger(newTriggerModel, newJob.Key);
            // determine if old triggerKey exists
            if (oldTriggerKey != null &&
                await Scheduler.CheckExists(oldTriggerKey.ToTriggerKey()).ConfigureAwait(false))
            {
                await Scheduler.UnscheduleJob(oldTriggerKey.ToTriggerKey())
                    .ConfigureAwait(false);
            }

            var existingTriggers = await Scheduler.GetTriggersOfJob(oJobKey).ConfigureAwait(false);

            // assign new job to all triggers
            var triggers = existingTriggers.Select(t =>
            {
                var b = t.GetTriggerBuilder().ForJob(newJob.Key);
                if (t.StartTimeUtc < DateTimeOffset.UtcNow)
                    b.StartNow();
                return b.Build();
            }).ToList();
            triggers.Add(trigger);

            // delete old job
            await Scheduler.DeleteJob(oJobKey).ConfigureAwait(false);

            // save new job with triggers
            await Scheduler.ScheduleJob(newJob, triggers, replace: true).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<JobDetailModel> GetJobDetail(string jobName, string groupName)
        {
            var jd = await Scheduler.GetJobDetail(new JobKey(jobName, groupName));

            if (jd == null)
                return null;

            return new JobDetailModel
            {
                Name = jd.Key.Name,
                Group = jd.Key.Group,
                Description = jd.Description,
                JobDataMap = jd.JobDataMap,
                JobClass = jd.JobType,
                IsDurable = jd.Durable
            };
        }

        [HttpGet]
        public async Task<TriggerDetailModel> GetTriggerDetail(string triggerName, string triggerGroup)
        {
            var trigger = await Scheduler.GetTrigger(new TriggerKey(triggerName, triggerGroup));

            if (trigger == null)
                return null;

            return CreateTriggerDetailModel(trigger);
        }

        [HttpGet]
        public async Task<bool> ContainsTriggerKey(string triggerName, string triggerGroup)
        {
            return await Scheduler.CheckExists(new TriggerKey(triggerName, triggerGroup));
        }

        [HttpGet]
        public async Task<bool> ContainsJobKey(string jobName, string jobGroup)
        {
            return await Scheduler.CheckExists(new JobKey(jobName, jobGroup));
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancelToken = default)
        {
            return await Scheduler.GetCalendarNames(cancelToken);
        }

        [HttpPost]
        public async Task PauseTrigger(string triggerName, string triggerGroup)
        {
            await Scheduler.PauseTrigger(triggerGroup == null ?
                new TriggerKey(triggerName) :
                new TriggerKey(triggerName, triggerGroup));
        }

        [HttpPost]
        public async Task ResumeTrigger(string triggerName, string triggerGroup)
        {
            await Scheduler.ResumeTrigger(triggerGroup == null ?
                new TriggerKey(triggerName) :
                new TriggerKey(triggerName, triggerGroup));
        }

        [HttpDelete]
        public async Task<bool> DeleteSchedule(ScheduleModel model)
        {
            if (model.JobName == null)
                return false;

            if (model.JobStatus == JobStatus.NoSchedule)
                return true;

            var jobKey = new JobKey(model.JobName, model.JobGroup);
            if (model.JobStatus == JobStatus.Error &&
                model.TriggerName == null)
            {
                Logger.LogInformation("Job [{jobGroup}.{jobName}] has no trigger name. " +
                    "Cannot UncheduleJob by trigger, will delete job directly.", jobKey.Group, jobKey.Name);
                return await Scheduler.DeleteJob(jobKey);
            }

            if (model.JobStatus == JobStatus.NoTrigger)
            {
                var triggers = await Scheduler.GetTriggersOfJob(jobKey);
                if (!triggers.Any())
                    return await Scheduler.DeleteJob(jobKey);
                else
                {
                    Logger.LogWarning("Cannot delete Job [{jobGroup}.{jobName}]. There are still {triggerCount}" +
                        " trigger(s) assigned to this job.", jobKey.Group, jobKey.Name,
                        triggers.Count);
                    return false;
                }
            }

            if (model.TriggerName == null)
                return false;

            var success = await Scheduler.UnscheduleJob(model.TriggerGroup == null ?
                new TriggerKey(model.TriggerName) :
                new TriggerKey(model.TriggerName, model.TriggerGroup));

            if (success)
            {
                var triggers = await Scheduler.GetTriggersOfJob(jobKey);
                if (!triggers.Any())
                {
                    Logger.LogInformation("UnscheduleJob [{jobGroup}.{jobName}] has no more triggers. " +
                        "Determine if job was deleted.", jobKey.Group, jobKey.Name);

                    if (await Scheduler.CheckExists(jobKey))
                    {
                        Logger.LogInformation("Manually delete job [{jobGroup}.{jobName}].", jobKey.Group, jobKey.Name);
                        return await Scheduler.DeleteJob(jobKey);
                    }
                }
            }

            return success;
        }

        [HttpPost]
        public async Task TriggerJob(string jobName, string jobGroup)
        {
            await Scheduler.TriggerJob(new JobKey(jobName, jobGroup));
        }

        [HttpGet]
        public async Task PauseAllSchedules()
        {
            await Scheduler.PauseAll();
        }

        [HttpGet]
        public async Task ResumeAllSchedules()
        {
            await Scheduler.ResumeAll();
        }

        [HttpGet]
        public async Task ShutdownScheduler()
        {
            await Scheduler.Shutdown();
        }

        [HttpGet]
        public async Task StartScheduler()
        {
            await Scheduler.Start();
        }

        [HttpGet]
        public async Task StandbyScheduler()
        {
            await Scheduler.Standby();
        }


        #region Private methods
        private async Task<ScheduleModel> CreateScheduleModel(IJobDetail jobDetail, ITrigger trigger, CancellationToken cancellationToken = default)
        {
            var triggerState = await Scheduler.GetTriggerState(trigger.Key);
            var runningTrigger = (await Scheduler.GetCurrentlyExecutingJobs(cancellationToken)).Where(context =>
                context.Trigger.Equals(trigger)).FirstOrDefault();

            return new ScheduleModel
            {
                JobName = jobDetail?.Key.Name,
                JobGroup = jobDetail?.Key.Group ?? "No Group",
                JobType = jobDetail?.JobType.ToString(),
                JobDescription = jobDetail?.Description,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
                TriggerDescription = trigger.Description,
                TriggerType = trigger.GetTriggerType(),
                TriggerTypeClassName = trigger.GetType().Name,
                NextTriggerTime = trigger.GetNextFireTimeUtc(),
                PreviousTriggerTime = trigger.GetPreviousFireTimeUtc(),
                JobStatus = runningTrigger != null ? JobStatus.Running : triggerState switch
                {
                    TriggerState.Paused => JobStatus.Paused,
                    TriggerState.None => JobStatus.NoTrigger,
                    TriggerState.Error => JobStatus.Error,
                    _ => JobStatus.Idle
                },
                TriggerDetail = CreateTriggerDetailModel(trigger)
            };
        }

        private async IAsyncEnumerable<ScheduleModel> GetScheduleModelsAsync(JobKey jobkey)
        {
            IJobDetail jobDetail = null;
            IReadOnlyCollection<ITrigger> jobTriggers = null;
            ScheduleModel exceptionJob = null;
            try
            {
                jobTriggers = await Scheduler.GetTriggersOfJob(jobkey);
                jobDetail = await Scheduler.GetJobDetail(jobkey);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Cannot GetScheduleModel of job [{jobGroup}.{jobName}]", jobkey.Group, jobkey.Name);
                exceptionJob = new ScheduleModel
                {
                    JobName = jobkey.Name,
                    JobGroup = jobkey.Group,
                    JobStatus = JobStatus.Error,
                    ExceptionMessage = ex.Message
                };
            }

            if (exceptionJob != null)
            {
                // job with exception
                if (jobTriggers == null || !jobTriggers.Any())
                {
                    exceptionJob.TriggerType = TriggerType.Unknown;
                    yield return exceptionJob;
                }
                else
                {
                    foreach (var trigger in jobTriggers)
                    {
                        var jobModel = await CreateScheduleModel(null, trigger);
                        jobModel.JobName = exceptionJob.JobName;
                        jobModel.JobGroup = exceptionJob.JobGroup;
                        jobModel.JobStatus = exceptionJob.JobStatus;
                        jobModel.ExceptionMessage = exceptionJob.ExceptionMessage;
                        yield return jobModel;
                    }
                }
            }
            else if (jobTriggers == null || !jobTriggers.Any())
            {
                yield return new ScheduleModel
                {
                    JobName = jobkey.Name,
                    JobGroup = jobkey.Group,
                    JobType = jobDetail?.JobType.ToString(),
                    JobStatus = JobStatus.NoTrigger
                };
            }
            else
            {
                foreach (var trigger in jobTriggers)
                {
                    yield return await CreateScheduleModel(jobDetail, trigger);
                }
            }
        }

        private TriggerDetailModel CreateTriggerDetailModel(ITrigger trigger)
        {
            var triggerType = trigger.GetTriggerType();

            var model = new TriggerDetailModel
            {
                Name = trigger.Key.Name,
                Group = trigger.Key.Group,
                Description = trigger.Description,
                TriggerDataMap = trigger.JobDataMap,
                EndDate = trigger.EndTimeUtc?.Date,
                EndTimeSpan = trigger.EndTimeUtc?.TimeOfDay,
                StartDate = trigger.StartTimeUtc.Date,
                StartTimeSpan = trigger.StartTimeUtc.TimeOfDay,
                StartTimezone = TimeZoneInfo.Utc,
                TriggerType = triggerType,
                ModifiedByCalendar = trigger.CalendarName,
                Priority = trigger.Priority,

            };

            switch (trigger.MisfireInstruction)
            {
                case MisfireInstruction.IgnoreMisfirePolicy:
                    model.MisfireAction = MisfireAction.IgnoreMisfirePolicy;
                    break;
                // comment out same as SmartPolicy
                //case MisfireInstruction.InstructionNotSet:
                //    model.MisfireAction = MisfireAction.InstructionNotSet;
                //    break;
                case MisfireInstruction.SmartPolicy:
                    model.MisfireAction = MisfireAction.SmartPolicy;
                    break;
            }

            switch (triggerType)
            {
                case TriggerType.Cron:
                    var cron = (ICronTrigger)trigger;
                    model.CronExpression = cron.CronExpressionString;
                    model.InTimeZone = cron.TimeZone;
                    switch (cron.MisfireInstruction)
                    {
                        case MisfireInstruction.CronTrigger.DoNothing:
                            model.MisfireAction = MisfireAction.DoNothing;
                            break;
                        case MisfireInstruction.CronTrigger.FireOnceNow:
                            model.MisfireAction = MisfireAction.FireOnceNow;
                            break;
                    }
                    break;
                case TriggerType.Daily:
                    var daily = (IDailyTimeIntervalTrigger)trigger;
                    foreach (var dow in daily.DaysOfWeek)
                    {
                        model.DailyDayOfWeek[(int)dow] = true;
                    }
                    switch (daily.MisfireInstruction)
                    {
                        case MisfireInstruction.DailyTimeIntervalTrigger.DoNothing:
                            model.MisfireAction = MisfireAction.DoNothing;
                            break;
                        case MisfireInstruction.DailyTimeIntervalTrigger.FireOnceNow:
                            model.MisfireAction = MisfireAction.FireOnceNow;
                            break;
                    }
                    model.RepeatCount = daily.RepeatCount;
                    model.TriggerInterval = daily.RepeatInterval;
                    model.TriggerIntervalUnit = daily.RepeatIntervalUnit.ToBlazoriseQuartzIntervalUnit();
                    model.InTimeZone = daily.TimeZone;
                    model.StartDailyTime = new TimeSpan(daily.StartTimeOfDay.Hour, daily.StartTimeOfDay.Minute, daily.StartTimeOfDay.Second);
                    model.EndDailyTime = new TimeSpan(daily.EndTimeOfDay.Hour, daily.EndTimeOfDay.Minute, daily.EndTimeOfDay.Second);
                    break;
                case TriggerType.Simple:
                    var simple = (ISimpleTrigger)trigger;
                    model = PopulateSimpleTrigger(simple, model);
                    break;
                case TriggerType.Calendar:
                    var calTrigger = (ICalendarIntervalTrigger)trigger;
                    switch (calTrigger.MisfireInstruction)
                    {
                        case MisfireInstruction.CalendarIntervalTrigger.DoNothing:
                            model.MisfireAction = MisfireAction.DoNothing;
                            break;
                        case MisfireInstruction.CalendarIntervalTrigger.FireOnceNow:
                            model.MisfireAction = MisfireAction.FireOnceNow;
                            break;
                    }
                    model.TriggerInterval = calTrigger.RepeatInterval;
                    model.TriggerIntervalUnit = calTrigger.RepeatIntervalUnit.ToBlazoriseQuartzIntervalUnit();
                    model.InTimeZone = calTrigger.TimeZone;
                    break;
            }

            return model;
        }

        private IJobDetail CreateJobDetail(JobDetailModel jobDetailModel)
        {
            ArgumentNullException.ThrowIfNull(jobDetailModel.JobClass);

            return JobBuilder.Create(jobDetailModel.JobClass)
                .WithIdentity(jobDetailModel.Name, jobDetailModel.Group)
                .WithDescription(jobDetailModel.Description)
                .UsingJobData(new JobDataMap(jobDetailModel.JobDataMap))
                .StoreDurably(jobDetailModel.IsDurable)
                .Build();
        }

        private ITrigger BuildTrigger(TriggerDetailModel triggerDetailModel, JobKey jobKey = null)
        {
            var tbldr = TriggerBuilder.Create()
                .WithIdentity(triggerDetailModel.Name, triggerDetailModel.Group)
                .WithDescription(triggerDetailModel.Description)
                .WithPriority(triggerDetailModel.Priority)
                .UsingJobData(new JobDataMap(triggerDetailModel.TriggerDataMap))
                .ModifiedByCalendar(triggerDetailModel.ModifiedByCalendar);

            if (jobKey != null)
            {
                tbldr.ForJob(jobKey);
            }

            var startTime = triggerDetailModel.StartDateTimeUtc;
            if (startTime.HasValue)
            {
                tbldr = tbldr.StartAt(startTime.Value);
            }
            else
            {
                tbldr = tbldr.StartNow();
            }

            tbldr.EndAt(triggerDetailModel.EndDateTimeUtc);

            switch (triggerDetailModel.TriggerType)
            {
                case TriggerType.Cron:
                    ArgumentNullException.ThrowIfNull(triggerDetailModel.CronExpression);
                    tbldr = tbldr.WithCronSchedule(triggerDetailModel.CronExpression,
                        x =>
                        {
                            switch (triggerDetailModel.MisfireAction)
                            {
                                case MisfireAction.DoNothing:
                                    x.WithMisfireHandlingInstructionDoNothing();
                                    break;
                                case MisfireAction.FireOnceNow:
                                    x.WithMisfireHandlingInstructionFireAndProceed();
                                    break;
                                case MisfireAction.IgnoreMisfirePolicy:
                                    x.WithMisfireHandlingInstructionIgnoreMisfires();
                                    break;
                            }
                            x.InTimeZone(triggerDetailModel.InTimeZone);
                        });
                    break;
                case TriggerType.Daily:
                    tbldr = tbldr.WithDailyTimeIntervalSchedule(x =>
                    {
                        switch (triggerDetailModel.MisfireAction)
                        {
                            case MisfireAction.DoNothing:
                                x.WithMisfireHandlingInstructionDoNothing();
                                break;
                            case MisfireAction.FireOnceNow:
                                x.WithMisfireHandlingInstructionFireAndProceed();
                                break;
                            case MisfireAction.IgnoreMisfirePolicy:
                                x.WithMisfireHandlingInstructionIgnoreMisfires();
                                break;
                        }
                        x.OnDaysOfTheWeek(triggerDetailModel.GetDailyOnDaysOfWeek());
                        if (triggerDetailModel.StartDailyTime.HasValue)
                        {
                            x.StartingDailyAt(triggerDetailModel.StartDailyTime.Value.ToTimeOfDay());
                        }
                        if (triggerDetailModel.EndDailyTime.HasValue)
                        {
                            x.EndingDailyAt(triggerDetailModel.EndDailyTime.Value.ToTimeOfDay());
                        }
                        x.InTimeZone(triggerDetailModel.InTimeZone);
                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            x.WithInterval(triggerDetailModel.TriggerInterval,
                                triggerDetailModel.TriggerIntervalUnit.Value.ToQuartzIntervalUnit());
                        }
                        if (triggerDetailModel.RepeatCount > 0)
                            x.WithRepeatCount(triggerDetailModel.RepeatCount);
                    });
                    break;
                case TriggerType.Simple:
                    tbldr = tbldr.WithSimpleSchedule(x =>
                    {
                        switch (triggerDetailModel.MisfireAction)
                        {
                            case MisfireAction.FireNow:
                                x.WithMisfireHandlingInstructionFireNow();
                                break;
                            case MisfireAction.RescheduleNextWithExistingCount:
                                x.WithMisfireHandlingInstructionNextWithExistingCount();
                                break;
                            case MisfireAction.RescheduleNextWithRemainingCount:
                                x.WithMisfireHandlingInstructionNextWithRemainingCount();
                                break;
                            case MisfireAction.RescheduleNowWithExistingRepeatCount:
                                x.WithMisfireHandlingInstructionNowWithExistingCount();
                                break;
                            case MisfireAction.RescheduleNowWithRemainingRepeatCount:
                                x.WithMisfireHandlingInstructionNowWithRemainingCount();
                                break;
                            case MisfireAction.IgnoreMisfirePolicy:
                                x.WithMisfireHandlingInstructionIgnoreMisfires();
                                break;
                        }

                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            TimeSpan timeSpan;
                            switch (triggerDetailModel.TriggerIntervalUnit.Value)
                            {
                                case IntervalUnit.Millisecond:
                                    timeSpan = TimeSpan.FromMilliseconds(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Second:
                                    timeSpan = TimeSpan.FromSeconds(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Minute:
                                    timeSpan = TimeSpan.FromMinutes(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Hour:
                                    timeSpan = TimeSpan.FromHours(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Day:
                                    timeSpan = TimeSpan.FromDays(triggerDetailModel.TriggerInterval);
                                    break;
                                default:
                                    throw new NotSupportedException(
                                        $"Interval unit {triggerDetailModel.TriggerIntervalUnit} is not supported for SimpleTrigger.");
                            }
                            x.WithInterval(timeSpan);
                        }

                        if (triggerDetailModel.RepeatForever)
                            x.RepeatForever();
                        else
                            x.WithRepeatCount(triggerDetailModel.RepeatCount);
                    });
                    break;
                case TriggerType.Calendar:
                    tbldr = tbldr.WithCalendarIntervalSchedule(x =>
                    {
                        switch (triggerDetailModel.MisfireAction)
                        {
                            case MisfireAction.DoNothing:
                                x.WithMisfireHandlingInstructionDoNothing();
                                break;
                            case MisfireAction.FireOnceNow:
                                x.WithMisfireHandlingInstructionFireAndProceed();
                                break;
                            case MisfireAction.IgnoreMisfirePolicy:
                                x.WithMisfireHandlingInstructionIgnoreMisfires();
                                break;
                        }

                        x.InTimeZone(triggerDetailModel.InTimeZone);
                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            x.WithInterval(triggerDetailModel.TriggerInterval,
                                triggerDetailModel.TriggerIntervalUnit.Value.ToQuartzIntervalUnit());
                        }
                    });
                    break;
            }

            return tbldr.Build();
        }

        private TriggerDetailModel PopulateSimpleTrigger(ISimpleTrigger simple, TriggerDetailModel model)
        {
            switch (simple.MisfireInstruction)
            {
                case MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount:
                    model.MisfireAction = MisfireAction.RescheduleNextWithExistingCount;
                    break;
                case MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount:
                    model.MisfireAction = MisfireAction.RescheduleNextWithRemainingCount;
                    break;
                case MisfireInstruction.SimpleTrigger.RescheduleNowWithExistingRepeatCount:
                    model.MisfireAction = MisfireAction.RescheduleNowWithExistingRepeatCount;
                    break;
                case MisfireInstruction.SimpleTrigger.RescheduleNowWithRemainingRepeatCount:
                    model.MisfireAction = MisfireAction.RescheduleNowWithRemainingRepeatCount;
                    break;
                case MisfireInstruction.SimpleTrigger.FireNow:
                    model.MisfireAction = MisfireAction.FireNow;
                    break;
            }
            if (simple.RepeatCount >= 0)
                model.RepeatCount = simple.RepeatCount;
            else
                model.RepeatForever = true;

            var total = simple.RepeatInterval.TotalHours;
            if (Math.Round(total) == total)
            {
                model.TriggerInterval = Convert.ToInt32(total);
                model.TriggerIntervalUnit = IntervalUnit.Hour;
            }
            else
            {
                total = simple.RepeatInterval.TotalMinutes;
                if (Math.Round(total) == total)
                {
                    model.TriggerInterval = Convert.ToInt32(total);
                    model.TriggerIntervalUnit = IntervalUnit.Minute;
                }
                else
                {
                    total = simple.RepeatInterval.TotalSeconds;
                    if (Math.Round(total) == total)
                    {
                        model.TriggerInterval = Convert.ToInt32(total);
                        model.TriggerIntervalUnit = IntervalUnit.Second;
                    }
                    //else
                    //{
                    //    total = simple.RepeatInterval.TotalMilliseconds;
                    //    if (Math.Round(total) == total)
                    //    {
                    //        model.TriggerInterval = Convert.ToInt32(total);
                    //        model.TriggerIntervalUnit = IntervalUnit.Millisecond;
                    //    }
                    //}
                }
            }

            return model;
        }

        #endregion Private methods

    }
}
