using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Matchers;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Syrna.QuartzAdmin.Authorization;
using Volo.Abp;

namespace Syrna.QuartzAdmin.Scheduler
{
    public class SchedulerAppService : QuartzAdminAppService, ISchedulerAppService
    {
        private IScheduler Scheduler => LazyServiceProvider.LazyGetRequiredService<IScheduler>();
        private ISchedulerDefinitionService SchedulerDefinitionService => LazyServiceProvider.LazyGetRequiredService<ISchedulerDefinitionService>();

        /// <summary>
        /// Getting meta data for a Scheduler.
        /// </summary>
        /// <returns>The Scheduler meta data.</returns>
        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<SchedulerDetails> GetSchedulerMetaData()
        {
            try
            {
                var metaData = await Scheduler.GetMetaData();
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<ApiResponse> StartSchedulerExt(int? delayMilliseconds = null)
        {
            try
            {
                if (delayMilliseconds == null)
                {
                    await Scheduler.Start();
                }
                else
                {
                    await Scheduler.StartDelayed(TimeSpan.FromMilliseconds(delayMilliseconds.Value));
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Delete)]
        public async Task<ApiResponse> ClearScheduler()
        {
            try
            {
                await Scheduler.Clear();
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
        [Authorize(QuartzAdminPermissions.Schedules.ShutDown)]
        public async Task<ApiResponse> ShutDownSchedulerExt(bool waitForJobsToComplete = false)
        {
            try
            {
                await Scheduler.Shutdown(waitForJobsToComplete);
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<List<ExecutingJobContext>> GetCurrentExecutingJobs()
        {
            try
            {
                await Scheduler.Clear();
                var metaData = await Scheduler.GetCurrentlyExecutingJobs();
                var model = metaData
                    .Select(ExecutingJobContext.Create)
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<List<ScheduleModel>> GetAllJobsAsync(ScheduleJobFilter filter)
        {
            var jobGroupNames = await Scheduler.GetJobGroupNames();
            var list = new List<ScheduleModel>(jobGroupNames.Count);
            foreach (var jobGrp in jobGroupNames)
            {
                if (filter is { IncludeSystemJobs: false })
                {
                    if (jobGrp == Constants.SYSTEM_GROUP)
                    {
                        continue;
                    }
                }

                var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGrp));

                foreach (var jobKey in jobKeys)
                {
                    foreach (var job in await GetScheduleModelsAsync(jobKey))
                    {
                        list.Add(job);
                    }
                }
            }
            return list;
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<ScheduleModel> GetScheduleModelAsync(ITrigger trigger)
        {
            var jobDetail = await Scheduler.GetJobDetail(trigger.JobKey);

            return await CreateScheduleModel(jobDetail, trigger);
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<IReadOnlyCollection<string>> GetJobGroups()
        {
            return (await Scheduler.GetJobGroupNames())
                .Where(n => n != Constants.SYSTEM_GROUP).ToList();
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<IReadOnlyCollection<string>> GetTriggerGroups()
        {
            return (await Scheduler.GetTriggerGroupNames()).
                Where(n => n != Constants.SYSTEM_GROUP).ToList();
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
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
                new("Jobs", jobCount),
                new("Triggers", triggerCount),
                new("Executing", executingCount),
                new("System Jobs", sysJobCount),
                new("System Triggers", sysTriggerCount)
            };
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<SchedulerMetaDataDto> GetMetadataAsync()
        {
            return SchedulerMetaDataDto.Create(await Scheduler.GetMetaData());
        }

        [HttpPost]
        [Authorize(QuartzAdminPermissions.Schedules.Create)]
        public async Task CreateSchedule(CreateScheduleArgs createScheduleArgs)
        {
            var trigger = BuildTrigger(createScheduleArgs.TriggerDetailModel);
            if (createScheduleArgs.JobDetailModel.Group != createScheduleArgs.TriggerDetailModel.Group)
            {
                //important
                createScheduleArgs.JobDetailModel.Group = createScheduleArgs.TriggerDetailModel.Group;
            }

            // Determine if job already exists
            if (await ContainsJobKey(createScheduleArgs.JobDetailModel.Name, createScheduleArgs.JobDetailModel.Group))
            {
                var existingJob = await Scheduler.GetJobDetail(new JobKey(createScheduleArgs.JobDetailModel.Name, createScheduleArgs.JobDetailModel.Group));
                if (existingJob != null)
                {
                    //await scheduler.GetTriggersOfJob(job.Key)
                    var jobTriggers = new List<ITrigger>(1)
                    {
                        trigger
                    };

                    await Scheduler.ScheduleJob(existingJob, jobTriggers.AsReadOnly(), true);
                    return;
                }
            }

            var job = CreateJobDetail(createScheduleArgs.JobDetailModel);

            await Scheduler.ScheduleJob(job, trigger);
        }

        [HttpPost]
        [Authorize(QuartzAdminPermissions.Schedules.Update)]
        public async Task UpdateSchedule(UpdateScheduleArgs args)
        {
            var oJobKey = args.OldJobKey.ToJobKey();

            var newJob = CreateJobDetail(args.NewJobModel);
            var trigger = BuildTrigger(args.NewTriggerModel, newJob.Key);
            // determine if old triggerKey exists
            if (args.OldTriggerKey != null &&
                await Scheduler.CheckExists(args.OldTriggerKey.ToTriggerKey()))
            {
                await Scheduler.UnscheduleJob(args.OldTriggerKey.ToTriggerKey());
            }

            var existingTriggers = await Scheduler.GetTriggersOfJob(oJobKey);

            // assign new job to all triggers
            var triggers = existingTriggers.Select(t =>
            {
                var b = t.GetTriggerBuilder().ForJob(newJob.Key);
                if (t.StartTimeUtc < DateTimeOffset.UtcNow)
                {
                    b.StartNow();
                }

                return b.Build();
            }).ToList();
            triggers.Add(trigger);

            // delete old job
            await Scheduler.DeleteJob(oJobKey);

            // save new job with triggers
            await Scheduler.ScheduleJob(newJob, triggers, replace: true);
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<JobDetailModel> GetJobDetail(string jobName, string groupName)
        {
            var jd = await Scheduler.GetJobDetail(new JobKey(jobName, groupName));

            if (jd == null)
            {
                return null;
            }

            return new JobDetailModel
            {
                Name = jd.Key.Name,
                Group = jd.Key.Group,
                Description = jd.Description,
                JobDataMap = jd.JobDataMap,
                JobClassName = jd.JobType.FullName,
                IsDurable = jd.Durable
            };
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<TriggerDetailModel> GetTriggerDetail(string triggerName, string triggerGroup)
        {
            var trigger = await Scheduler.GetTrigger(new TriggerKey(triggerName, triggerGroup));

            if (trigger == null)
            {
                return null;
            }

            return CreateTriggerDetailModel(trigger);
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<bool> ContainsTriggerKey(string triggerName, string triggerGroup)
        {
            return await Scheduler.CheckExists(new TriggerKey(triggerName, triggerGroup));
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<bool> ContainsJobKey(string jobName, string jobGroup)
        {
            return await Scheduler.CheckExists(new JobKey(jobName, jobGroup));
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancelToken = default)
        {
            return await Scheduler.GetCalendarNames(cancelToken);
        }

        [HttpPost]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task PauseTrigger(string triggerName, string triggerGroup)
        {
            await Scheduler.PauseTrigger(triggerGroup == null ?
                new TriggerKey(triggerName) :
                new TriggerKey(triggerName, triggerGroup));
        }

        [HttpPost]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task ResumeTrigger(string triggerName, string triggerGroup)
        {
            await Scheduler.ResumeTrigger(triggerGroup == null ?
                new TriggerKey(triggerName) :
                new TriggerKey(triggerName, triggerGroup));
        }

        [HttpDelete]
        [Authorize(QuartzAdminPermissions.Schedules.Delete)]
        public async Task<bool> DeleteSchedule(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                return false;
            }

            if (model.JobStatus == JobStatus.NoSchedule)
            {
                return true;
            }

            var jobKey = new JobKey(model.JobName, model.JobGroup);
            if (model.JobStatus == JobStatus.Error &&
                model.TriggerName == null)
            {
                Logger.LogInformation("Job [{jobGroup}.{jobName}] has no trigger name. " +
                    "Cannot UnscheduleJob by trigger, will delete job directly.", jobKey.Group, jobKey.Name);
                return await Scheduler.DeleteJob(jobKey);
            }

            if (model.JobStatus == JobStatus.NoTrigger)
            {
                var triggers = await Scheduler.GetTriggersOfJob(jobKey);
                if (!triggers.Any())
                {
                    return await Scheduler.DeleteJob(jobKey);
                }
                else
                {
                    Logger.LogWarning("Cannot delete Job [{jobGroup}.{jobName}]. There are still {triggerCount}" +
                        " trigger(s) assigned to this job.", jobKey.Group, jobKey.Name,
                        triggers.Count);
                    return false;
                }
            }

            if (model.TriggerName == null)
            {
                return false;
            }

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
        [Authorize(QuartzAdminPermissions.Schedules.Trigger)]
        public async Task TriggerJob(string jobName, string jobGroup)
        {
            await Scheduler.TriggerJob(new JobKey(jobName, jobGroup));
        }

        /// <summary>
        /// Interrupts the execution of a job.
        /// </summary>
        /// <param name="fireInstanceId">The id of the running job instance.</param>
        /// <returns>True if the job is interrupted, otherwise false.</returns>
        /// <response code="200">True if job was interrupted.</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpPost]
        [Authorize(QuartzAdminPermissions.Schedules.Interrupt)]
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

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task PauseAllSchedules()
        {
            await Scheduler.PauseAll();
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task ResumeAllSchedules()
        {
            await Scheduler.ResumeAll();
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.ShutDown)]
        public async Task ShutdownScheduler()
        {
            await Scheduler.Shutdown();
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public async Task StartScheduler()
        {
            await Scheduler.Start();
        }

        [HttpGet]
        [Authorize(QuartzAdminPermissions.Schedules.Standby)]
        public async Task StandbyScheduler()
        {
            await Scheduler.Standby();
        }


        #region Private methods
        private async Task<ScheduleModel> CreateScheduleModel(IJobDetail jobDetail, ITrigger trigger, CancellationToken cancellationToken = default)
        {
            var triggerState = await Scheduler.GetTriggerState(trigger.Key, cancellationToken);
            var runningTrigger = (await Scheduler.GetCurrentlyExecutingJobs(cancellationToken)).FirstOrDefault(context => context.Trigger.Equals(trigger));

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

        private async Task<List<ScheduleModel>> GetScheduleModelsAsync(JobKey jobKey)
        {
            IJobDetail jobDetail = null;
            IReadOnlyCollection<ITrigger> jobTriggers = null;
            ScheduleModel exceptionJob = null;
            var list = new List<ScheduleModel>();
            try
            {
                jobTriggers = await Scheduler.GetTriggersOfJob(jobKey);
                jobDetail = await Scheduler.GetJobDetail(jobKey);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Cannot GetScheduleModel of job [{jobGroup}.{jobName}]", jobKey.Group, jobKey.Name);
                exceptionJob = new ScheduleModel
                {
                    JobName = jobKey.Name,
                    JobGroup = jobKey.Group,
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
                    list.Add(exceptionJob);
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
                        list.Add(jobModel);
                    }
                }
            }
            else if (jobTriggers == null || !jobTriggers.Any())
            {
                var sm = new ScheduleModel
                {
                    JobName = jobKey.Name,
                    JobGroup = jobKey.Group,
                    JobType = jobDetail?.JobType.ToString(),
                    JobStatus = JobStatus.NoTrigger
                };
                list.Add(sm);
            }
            else
            {
                foreach (var trigger in jobTriggers)
                {
                    list.Add(await CreateScheduleModel(jobDetail, trigger));
                }
            }
            return list;
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
                InTimeZoneId = TimeZoneInfo.Utc.Id,
                TriggerType = triggerType,
                ModifiedByCalendar = trigger.CalendarName,
                Priority = trigger.Priority,

            };

            model.MisfireAction = trigger.MisfireInstruction switch
            {
                MisfireInstruction.IgnoreMisfirePolicy => MisfireAction.IgnoreMisfirePolicy,
                // comment out same as SmartPolicy
                //case MisfireInstruction.InstructionNotSet:
                //    model.MisfireAction = MisfireAction.InstructionNotSet;
                //    break;
                MisfireInstruction.SmartPolicy => MisfireAction.SmartPolicy,
                _ => model.MisfireAction
            };

            switch (triggerType)
            {
                case TriggerType.Cron:
                    var cron = (ICronTrigger)trigger;
                    model.CronExpression = cron.CronExpressionString;
                    model.InTimeZoneId = cron.TimeZone.Id;
                    model.MisfireAction = cron.MisfireInstruction switch
                    {
                        MisfireInstruction.CronTrigger.DoNothing => MisfireAction.DoNothing,
                        MisfireInstruction.CronTrigger.FireOnceNow => MisfireAction.FireOnceNow,
                        _ => model.MisfireAction
                    };
                    break;
                case TriggerType.Daily:
                    var daily = (IDailyTimeIntervalTrigger)trigger;
                    foreach (var dow in daily.DaysOfWeek)
                    {
                        model.DailyDayOfWeek[(int)dow] = true;
                    }

                    model.MisfireAction = daily.MisfireInstruction switch
                    {
                        MisfireInstruction.DailyTimeIntervalTrigger.DoNothing => MisfireAction.DoNothing,
                        MisfireInstruction.DailyTimeIntervalTrigger.FireOnceNow => MisfireAction.FireOnceNow,
                        _ => model.MisfireAction
                    };
                    model.RepeatCount = daily.RepeatCount;
                    model.TriggerInterval = daily.RepeatInterval;
                    model.TriggerIntervalUnit = daily.RepeatIntervalUnit.ToQuartzAdminIntervalUnit();
                    model.InTimeZoneId = daily.TimeZone.Id;
                    model.StartDailyTime = new TimeSpan(daily.StartTimeOfDay.Hour, daily.StartTimeOfDay.Minute, daily.StartTimeOfDay.Second);
                    model.EndDailyTime = new TimeSpan(daily.EndTimeOfDay.Hour, daily.EndTimeOfDay.Minute, daily.EndTimeOfDay.Second);
                    break;
                case TriggerType.Simple:
                    var simple = (ISimpleTrigger)trigger;
                    model = PopulateSimpleTrigger(simple, model);
                    break;
                case TriggerType.Calendar:
                    var calTrigger = (ICalendarIntervalTrigger)trigger;
                    model.MisfireAction = calTrigger.MisfireInstruction switch
                    {
                        MisfireInstruction.CalendarIntervalTrigger.DoNothing => MisfireAction.DoNothing,
                        MisfireInstruction.CalendarIntervalTrigger.FireOnceNow => MisfireAction.FireOnceNow,
                        _ => model.MisfireAction
                    };
                    model.TriggerInterval = calTrigger.RepeatInterval;
                    model.TriggerIntervalUnit = calTrigger.RepeatIntervalUnit.ToQuartzAdminIntervalUnit();
                    model.InTimeZoneId = calTrigger.TimeZone.Id;
                    break;
            }

            return model;
        }

        private IJobDetail CreateJobDetail(JobDetailModel jobDetailModel)
        {
            ArgumentNullException.ThrowIfNull(jobDetailModel.JobClassName);
            var type = SchedulerDefinitionService.FindType(jobDetailModel.JobClassName);
            return JobBuilder.Create(type)
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
                            x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(triggerDetailModel.InTimeZoneId));
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
                        x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(triggerDetailModel.InTimeZoneId));
                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            x.WithInterval(triggerDetailModel.TriggerInterval,
                                triggerDetailModel.TriggerIntervalUnit.Value.ToQuartzIntervalUnit());
                        }
                        if (triggerDetailModel.RepeatCount > 0)
                        {
                            x.WithRepeatCount(triggerDetailModel.RepeatCount);
                        }
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
                        {
                            x.RepeatForever();
                        }
                        else
                        {
                            x.WithRepeatCount(triggerDetailModel.RepeatCount);
                        }
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

                        x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(triggerDetailModel.InTimeZoneId));
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
            {
                model.RepeatCount = simple.RepeatCount;
            }
            else
            {
                model.RepeatForever = true;
            }

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
