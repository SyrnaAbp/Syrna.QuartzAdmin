using System;
using System.ComponentModel.DataAnnotations;

namespace Syrna.QuartzAdmin.ExecutionHistory
{
    [Serializable]
    public class ExecutionHistoryEntry
    {
        public string FireInstanceId { get; set; }
        public string SchedulerInstanceId { get; set; }
        public string SchedulerName { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public LogType LogType { get; set; }

        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }

        public DateTimeOffset? ScheduledFireTimeUtc { get; set; }
        public DateTimeOffset FireTimeUtc { get; set; }
        public bool Recovering { get; set; }
        public bool? IsVetoed { get; set; }
        public DateTimeOffset? FinishedTimeUtc { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public string Result { get; set; }
        public bool? IsException { get; set; }
        public TimeSpan? JobRunTime { get; set; }
        public ExecutionHistoryDetail ExecutionHistoryDetail { get; set; }
        /// <summary>
        /// Indicate whether the execution is successful or not.
        /// If <see cref="LogType"/> is <see cref="LogType.ScheduleJob"/>, it may have value:
        /// <para>true - If job does not return IsSuccess or when execution completed successfully</para>
        /// <para>false - Execution completed but return code is error or job throw an exception</para>
        /// <para>null - Job still running or terminated unexpectedly.</para>
        /// If <see cref="LogType"/> is not <see cref="LogType.ScheduleJob"/> value will be null.
        /// </summary>
        public bool? IsSuccess { get; set; }
        /// <summary>
        /// Return code of execution.
        /// <para>Ex.</para>
        /// <para>for HTTP call - 200, 404, 500 etc.</para>
        /// <para>for command line - 0 = success, -1 = failed</para>
        /// </summary>
        [MaxLength(28)]
        public string ReturnCode { get; set; }
    }
}
