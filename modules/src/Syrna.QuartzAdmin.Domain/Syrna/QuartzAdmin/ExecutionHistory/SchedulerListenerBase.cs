using Quartz;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Syrna.QuartzAdmin.ExecutionHistory;

public abstract class SchedulerListenerBase : DomainService, ISchedulerListener
{
    public virtual Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulerShutdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulerStarted(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulerStarting(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task SchedulingDataCleared(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task TriggersPaused(string triggerGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task TriggersResumed(string triggerGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
