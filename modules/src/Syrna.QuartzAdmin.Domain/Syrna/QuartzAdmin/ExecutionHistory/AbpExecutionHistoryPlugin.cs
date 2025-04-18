using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace Syrna.QuartzAdmin.ExecutionHistory;

public class AbpExecutionHistoryPlugin : SchedulerListenerBase, ISchedulerPlugin, IJobListener, ITriggerListener
{
    private const int ResultMaxLength = 8000;

    private IScheduler _scheduler = null!;
    private IExecutionHistoryStore _store = null!;

    public Type StoreType { get; set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public Task Initialize(string pluginName, IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        Name = pluginName;
        _scheduler = scheduler;
        _scheduler.ListenerManager.AddJobListener(this, EverythingMatcher<JobKey>.AllJobs());

        return Task.FromResult(0);
    }

    public override async Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        var jKey = trigger.JobKey;
        var tKey = trigger.Key;
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            LogType = LogType.Trigger,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            Result = "Job scheduled"
        };
        await _store.Save(entry);
    }

    public override async Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        var jKey = trigger.JobKey;
        var tKey = trigger.Key;
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            LogType = LogType.Trigger,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            Result = "Trigger ended"
        };
        await _store.Save(entry);
    }

    public override async Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        var tKey = triggerKey;
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = LogType.Trigger,
            Result = "Trigger resumed"
        };
        if (entry.JobName == null &&
            entry.TriggerName != null &&
            entry.TriggerGroup != null)
        {
            // when there is no job name but has trigger name
            // try to determine the job name
            var trigger = await _scheduler.GetTrigger(new TriggerKey(entry.TriggerName, entry.TriggerGroup),
                cancellationToken);
            if (trigger != null)
            {
                entry.JobName = trigger.JobKey.Name;
                entry.JobGroup = trigger.JobKey.Group;
            }
        }
        await _store.Save(entry);
    }

    public override async Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        var tKey = triggerKey;
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = LogType.Trigger,
            Result = "Trigger paused"
        };
        if (entry.JobName == null &&
            entry.TriggerName != null &&
            entry.TriggerGroup != null)
        {
            // when there is no job name but has trigger name
            // try to determine the job name
            var trigger = await _scheduler.GetTrigger(new TriggerKey(entry.TriggerName, entry.TriggerGroup),
                cancellationToken);
            if (trigger != null)
            {
                entry.JobName = trigger.JobKey.Name;
                entry.JobGroup = trigger.JobKey.Group;
            }
        }
        await _store.Save(entry);
    }

    public async Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        var jKey = trigger.JobKey;
        var tKey = trigger.Key;
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            LogType = LogType.Trigger,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            Result = "Trigger misfired"
        };
        await _store.Save(entry);
    }

    public override async Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
    {
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            LogType = LogType.System,
            IsException = true,
            ErrorMessage = msg,
            ExecutionHistoryDetail = new()
            {
                ErrorStackTrace = cause.NonNullStackTrace()
            }
        };
        await _store.Save(entry);
    }

    public override async Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var jKey = jobKey;
        var entry = new ExecutionHistoryEntry
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = LogType.System,
            Result = "Job interrupted"
        };
        await _store.Save(entry);
    }

    public override async Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var jKey = jobKey;
        var entry = new ExecutionHistoryEntry()
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = LogType.System,
            Result = "Job deleted"
        };
        await _store.Save(entry);
    }

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var log = CreateScheduleJobLogEntry(context, defaultIsSuccess: false);
        log.IsVetoed = true;
        await _store.Save(log);
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        var entry = CreateScheduleJobLogEntry(context, jobException, true);
        entry.FinishedTimeUtc = DateTime.UtcNow;
        await _store.Save(entry);

        if (jobException == null)
        {
            await _store.IncrementTotalJobsExecuted();
        }
        else
        {
            await _store.IncrementTotalJobsFailed();
        }
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = CreateScheduleJobLogEntry(context);
            await _store.Save(entry);
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
        }
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin start");
        _store = _scheduler.Context.GetExecutionHistoryStore();

        if (_store == null)
        {
            throw new AbpException(nameof(StoreType) + " is not set.");
        }

        _store.SchedulerName = _scheduler.SchedulerName;

        if (_store is AbpExecutionHistoryStore abpStore)
        {
            await abpStore.InitializeSummaryAsync();
        }

        //await _store.Purge();
        await PrepareAutoJobs();
        await RegisterAutoJobsAsync(cancellationToken);
        await MarkIncompleteExecution(cancellationToken);
    }

    private async Task MarkIncompleteExecution(CancellationToken stoppingToken)
    {
        try
        {
            await _store.MarkExecutingJobAsIncomplete(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Prevent throwing if stoppingToken was signaled
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while updating executing status to incomplete status.");
        }
    }

    private async Task PrepareAutoJobs()
    {
        var type = typeof(IJob);
        var types = AutoJobsListHelper.GetQuartzAdminJobs();
        foreach (var t in types)
        {
            var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
            await PrepareAutoJob(t, () =>
            {
                if (!so.Manual)
                {
                    var tb = TriggerBuilder.Create();
                    tb.WithSimpleSchedule(x =>
                    {
                        x.WithInterval(so.WithInterval);
                        if (so.RepeatCount > 0)
                        {
                            x.WithRepeatCount(so.RepeatCount);

                        }
                        else
                        {
                            x.RepeatForever();
                        }
                    });
                    if (so.StartAt == DateTimeOffset.MinValue)
                    {
                        tb.StartNow();
                    }
                    else
                    {
                        tb.StartAt(so.StartAt);
                    }

                    var tk = new TriggerKey(!string.IsNullOrEmpty(so.TriggerName) ? so.TriggerName : $"{t.Name}'s Trigger");
                    if (!string.IsNullOrEmpty(so.TriggerGroup))
                    {
                        so.TriggerGroup = so.TriggerGroup;
                    }
                    tb.WithIdentity(tk);
                    tb.WithDescription(so.TriggerDescription ?? $"{t.Name}'s Trigger,full name is {t.FullName}");
                    if (so.Priority > 0)
                    {
                        tb.WithPriority(so.Priority);
                    }

                    return tb;
                }
                else
                {
                    return null;
                }
            });
        }
    }

    private async Task PrepareAutoJob(Type t, Func<TriggerBuilder> triggerBuildersFunc)
    {
        var lst = new List<TriggerBuilder>();
        var tb = triggerBuildersFunc?.Invoke();
        if (tb != null)
        {
            lst.Add(tb);
        }
        await PrepareAutoJob(t, lst);
    }

    private IEnumerable<IScheduleJob> ScheduleJobs => LazyServiceProvider.LazyGetRequiredService<IEnumerable<IScheduleJob>>();

    private async Task PrepareAutoJob(Type t, IEnumerable<TriggerBuilder> triggerBuilders)
    {
        var job = (from js in ScheduleJobs where js.JobDetail.JobType == t select js).ToList();
        if (job.Any())
        {
            var scheduleJob = job.First();
            var items = (List<ITrigger>)scheduleJob.Triggers;
            triggerBuilders.ToList().ForEach(triggerBuilder =>
            {
                items.Add(triggerBuilder.ForJob(scheduleJob.JobDetail).Build());
            });
        }
        await Task.CompletedTask;
    }

    private async Task RegisterAutoJobsAsync(CancellationToken cancellationToken)
    {
        if (ScheduleJobs == null || !ScheduleJobs.Any())
        {
            return;
        }

        foreach (var scheduleJob in ScheduleJobs)
        {
            var isNewJob = true;
            foreach (var trigger in scheduleJob.Triggers)
            {

                if (isNewJob)
                {
                    if (!(await _scheduler.CheckExists(scheduleJob.JobDetail.Key, cancellationToken)))
                    {
                        await _scheduler.ScheduleJob(scheduleJob.JobDetail, trigger, cancellationToken);
                    }
                }
                else
                {
                    if (!(await _scheduler.CheckExists(trigger.Key, cancellationToken)))
                    {
                        await _scheduler.ScheduleJob(trigger, cancellationToken);
                    }
                }
                isNewJob = false;
            }
        }
    }

    public Task Shutdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private ExecutionHistoryEntry CreateScheduleJobLogEntry(IJobExecutionContext context, JobExecutionException jobException = null, bool? defaultIsSuccess = null)
    {
        var log = new ExecutionHistoryEntry
        {
            SchedulerInstanceId = _scheduler.SchedulerInstanceId,
            SchedulerName = _scheduler.SchedulerName,
            FireInstanceId = context.FireInstanceId,
            JobGroup = context.JobDetail.Key.Group,
            JobName = context.JobDetail.Key.Name,
            TriggerName = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group,
            FireTimeUtc = context.FireTimeUtc,
            ScheduledFireTimeUtc = context.ScheduledFireTimeUtc,
            RetryCount = context.RefireCount,
            JobRunTime = context.JobRunTime,
            LogType = LogType.ScheduleJob
        };
        var logDetail = new ExecutionHistoryDetail();
        if (context.CancellationToken.IsCancellationRequested)
        {
            log.ReturnCode = "-1";
            log.IsSuccess = false;
            log.IsException = true;
            log.ErrorMessage = "Canceled by user";

            logDetail.ExecutionDetails = "Canceled by user";
            //logDetail.ErrorCode = -1;
            log.ExecutionHistoryDetail = logDetail;
        }
        else
        {

            log.ReturnCode = context.GetReturnCode();
            log.IsSuccess = context.GetIsSuccess();

            log.IsSuccess ??= defaultIsSuccess;

            var execDetail = context.GetExecutionDetails();
            if (!string.IsNullOrEmpty(execDetail))
            {
                logDetail.ExecutionDetails = execDetail;
                log.ExecutionHistoryDetail = logDetail;
            }

            if (jobException != null)
            {
                log.ErrorMessage = jobException.Message;
                log.ExecutionHistoryDetail = logDetail;
                logDetail.ErrorCode = jobException.HResult;
                logDetail.ErrorStackTrace = jobException.ToString();
                logDetail.ErrorHelpLink = jobException.HelpLink;

                log.ReturnCode ??= jobException.HResult.ToString();

                log.IsException = true;
                log.IsSuccess = false;
            }
            else
            {
                if (context.Result != null)
                {
                    var result = Convert.ToString(context.Result, CultureInfo.InvariantCulture);
                    log.Result = result?.Substring(0, Math.Min(result.Length, ResultMaxLength));
                }
            }
        }

        return log;
    }

    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
    public override Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin SchedulerInStandbyMode");
        return Task.CompletedTask;
    }

    public override Task SchedulerStarted(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin SchedulerStarted");
        return Task.CompletedTask;
    }

    public override Task SchedulerStarting(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin SchedulerStarting");
        return Task.CompletedTask;
    }

    public override Task SchedulerShutdown(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin SchedulerShutdown");
        return Task.CompletedTask;
    }

    public override Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin SchedulerShuttingDown");
        return Task.CompletedTask;
    }

    public override Task SchedulingDataCleared(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Plugin SchedulingDataCleared");
        return Task.CompletedTask;
    }
}
