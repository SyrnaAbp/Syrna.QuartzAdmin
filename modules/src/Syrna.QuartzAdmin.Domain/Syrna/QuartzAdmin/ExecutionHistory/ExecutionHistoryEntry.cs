using System;

namespace Syrna.QuartzAdmin.ExecutionHistory
{
    [Serializable]
    public class ExecutionHistoryEntry
    {
        public string FireInstanceId { get; set; }
        public string SchedulerInstanceId { get; set; }
        public string SchedulerName { get; set; }
        public string Job { get; set; }
        public string Trigger { get; set; }
        public DateTimeOffset? ScheduledFireTimeUtc { get; set; }
        public DateTimeOffset ActualFireTimeUtc { get; set; }
        public bool Recovering { get; set; }
        public bool Vetoed { get; set; }
        public DateTimeOffset? FinishedTimeUtc { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
