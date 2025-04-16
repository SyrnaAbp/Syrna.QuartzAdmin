using Quartz;
using Quartz.Listener;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System.Reflection;
using Volo.Abp.DependencyInjection;

public abstract class SchedulerListenerBase : SchedulerListenerSupport
{
    public IAbpLazyServiceProvider LazyServiceProvider { get; set; }
    //protected ILoggerFactory LoggerFactory => LazyServiceProvider.LazyGetRequiredService<ILoggerFactory>();
    //protected ILogger Logger => LazyServiceProvider.LazyGetService<ILogger>(provider => LoggerFactory?.CreateLogger(GetType().FullName!) ?? NullLogger.Instance);
}

public class SampleSchedulerListener : SchedulerListenerBase
{
    private readonly ILogger<SampleSchedulerListener> logger;
    protected IEnumerable<IScheduleJob> _scheduleJobs => LazyServiceProvider.LazyGetRequiredService<IEnumerable<IScheduleJob>>();
    protected IScheduler _scheduler => LazyServiceProvider.LazyGetRequiredService<IScheduler>();

    public SampleSchedulerListener(ILogger<SampleSchedulerListener> logger)
    {
        this.logger = logger;
    }

    public override async Task SchedulerStarted(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Observed scheduler start");
        await UseQuartzAdmin();
        await StartAsync(cancellationToken);
        await Task.CompletedTask;
    }

    public async Task UseQuartzAdmin()
    {
        var type = typeof(IJob);
        var types = AutoJobsListHelper.GetQuartzAdminJobs();
        foreach (var t in types)
        {
            var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
            await UseQuartzJob(t, () =>
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

    public async Task UseQuartzJob(Type t, Func<TriggerBuilder> triggerBuilders_func)
    {
        var lst = new List<TriggerBuilder>();
        var tb = triggerBuilders_func?.Invoke();
        if (tb != null)
        {
            lst.Add(tb);
        }
        await UseQuartzJob(t, lst);
    }

    public async Task UseQuartzJob(Type t, IEnumerable<TriggerBuilder> triggerBuilders)
    {
        var job = from js in _scheduleJobs where js.JobDetail.JobType == t select js;
        if (job.Any())
        {
            var scheduleJob = job.First();
            var lstgs = (List<ITrigger>)scheduleJob.Triggers;
            triggerBuilders.ToList().ForEach(triggerBuilder =>
            {
                lstgs.Add(triggerBuilder.ForJob(scheduleJob.JobDetail).Build());
            });
        }
        await Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_scheduleJobs == null || !_scheduleJobs.Any())
        {
            return;
        }

        foreach (var scheduleJob in _scheduleJobs)
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
}