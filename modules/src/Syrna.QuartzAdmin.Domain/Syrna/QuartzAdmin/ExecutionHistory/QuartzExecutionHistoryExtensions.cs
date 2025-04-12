namespace Syrna.QuartzAdmin.ExecutionHistory;

public static class QuartzExecutionHistoryExtensions
{
    public static QuartzExecutionHistory ToEntity(this ExecutionHistoryEntry entry, QuartzExecutionHistory entity)
    {
        entity.SchedulerInstanceId = entry.SchedulerInstanceId;
        entity.SchedulerName = entry.SchedulerName;
        entity.JobName = entry.JobName;
        entity.JobGroup = entry.JobGroup;
        entity.TriggerName = entry.TriggerName;
        entity.TriggerGroup = entry.TriggerGroup;
        entity.ScheduledFireTimeUtc = entry.ScheduledFireTimeUtc;
        entity.FireTimeUtc = entry.FireTimeUtc;
        entity.Recovering = entry.Recovering;
        entity.IsVetoed = entry.IsVetoed;
        entity.FinishedTimeUtc = entry.FinishedTimeUtc;
        entity.ErrorMessage = entry.ErrorMessage;
        entity.RetryCount = entry.RetryCount;
        entity.Result = entry.Result;
        entity.LogType = entry.LogType;
        entity.IsSuccess = entry.IsSuccess;
        entity.IsException = entry.IsException;
        entity.JobRunTime = entry.JobRunTime;
        entity.ExecutionHistoryDetail = entry.ExecutionHistoryDetail;
        entity.ReturnCode = entry.ReturnCode;
        return entity;
    }

    public static ExecutionHistoryEntry ToEntry(this QuartzExecutionHistory entity)
    {
        return new ExecutionHistoryEntry()
        {
            FireInstanceId = entity.FireInstanceId,
            SchedulerInstanceId = entity.SchedulerInstanceId,
            SchedulerName = entity.SchedulerName,
            JobName = entity.JobName,
            JobGroup = entity.JobGroup,
            TriggerName = entity.TriggerName,
            TriggerGroup = entity.TriggerGroup,
            ScheduledFireTimeUtc = entity.ScheduledFireTimeUtc,
            FireTimeUtc = entity.   FireTimeUtc,
            Recovering = entity.Recovering,
            IsVetoed = entity.IsVetoed,
            FinishedTimeUtc = entity.FinishedTimeUtc,
            ErrorMessage = entity.ErrorMessage,
            RetryCount = entity.RetryCount,
            Result = entity.Result,
            LogType = entity.LogType,
            IsSuccess = entity.IsSuccess,
            IsException = entity.IsException,
            JobRunTime = entity.JobRunTime,
            ExecutionHistoryDetail = entity.ExecutionHistoryDetail,
            ReturnCode = entity.ReturnCode
        };
    }
}
