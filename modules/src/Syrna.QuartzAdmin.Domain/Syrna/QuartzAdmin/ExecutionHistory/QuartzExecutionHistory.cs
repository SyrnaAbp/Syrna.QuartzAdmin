using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace Syrna.QuartzAdmin.ExecutionHistory;
public class QuartzExecutionHistory : BasicAggregateRoot<long>
{
    public string FireInstanceId { get; protected set; } = null!;
    public LogType LogType { get; set; }

    public string SchedulerInstanceId { get; set; } = null!;

    public string SchedulerName { get; set; } = null!;

    public string JobName { get; set; }
    public string JobGroup { get; set; }

    public string TriggerName { get; set; }
    public string TriggerGroup { get; set; }

    public DateTimeOffset? ScheduledFireTimeUtc { get; set; }

    public DateTimeOffset FireTimeUtc { get; set; }

    public bool Recovering { get; set; }
    public int RetryCount { get; set; }
    public string Result { get; set; }

    public bool? IsVetoed { get; set; }

    public DateTimeOffset? FinishedTimeUtc { get; set; }
    
    public DateTimeOffset DateAddedUtc { get; set; }

    public string ErrorMessage { get; set; }
    public bool? IsSuccess { get; set; }
    public bool? IsException { get; set; }
    public TimeSpan? JobRunTime { get; set; }
    public ExecutionHistoryDetail ExecutionHistoryDetail { get; set; }
    public string ReturnCode { get; set; }

    protected QuartzExecutionHistory()
    {
        DateAddedUtc = DateTimeOffset.UtcNow;
    }

    public QuartzExecutionHistory(long id, string fireInstanceId) : base(id)
    {
        FireInstanceId = fireInstanceId;
    }

    public QuartzExecutionHistory(string fireInstanceId) : this()
    {
        FireInstanceId = fireInstanceId;
    }
}
