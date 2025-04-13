using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.ExecutionLog
{
    public interface IExecutionLogAppService : IApplicationService
    {
        Task<DataEnvelope<ExecutionLogDto>> GetLatestExecutionLog(LatestExecutionLogReadArgs args);
        Task<DataEnvelope<ExecutionLogDto>> GetExecutionLogs(ExecutionLogReadArgs args);
        Task<IList<string>> GetJobNames();
        Task<IList<string>> GetJobGroups();
        Task<IList<string>> GetTriggerNames();
        Task<IList<string>> GetTriggerGroups();
        /// <summary>
        /// Returns job execution summary. Number of success, failed,
        /// executing and interrupted jobs of given date range.
        /// </summary>
        /// <param name="startTimeUtc"></param>
        /// <param name="endTimeUtc">inclusive</param>
        /// <returns></returns>
        Task<JobExecutionStatusSummaryModel> GetJobExecutionStatusSummary(JobExecutionStatusSummaryReadArgs args);
    }
    public class JobExecutionStatusSummaryReadArgs
    {
        public DateTimeOffset? StartTimeUtc { get; set; }
        public DateTimeOffset? EndTimeUtc { get; set; }
    }
    public class LatestExecutionLogReadArgs
    {
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
        public PageMetadata PageMetadata { get; set; } = null;
        public long FirstLogId { get; set; } = 0;
        public HashSet<LogType> LogTypes { get; set; } = null;
    }
    public class ExecutionLogReadArgs
    {
        public ExecutionLogFilter Filter { get; set; } = null;
        public PageMetadata PageMetadata { get; set; } = null;
        public long FirstLogId { get; set; } = 0;
    }
}