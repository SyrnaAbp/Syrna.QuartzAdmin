using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.ExecutionLog
{
    public interface IExecutionLogAppService : IApplicationService
    {
        Task<DataEnvelope<ExecutionLogDto>> GetLatestExecutionLog(string jobName, string jobGroup, string triggerName, string triggerGroup, PageMetadata pageMetadata = null, long firstLogId = 0, HashSet<LogType> logTypes = null);
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
        Task<JobExecutionStatusSummaryModel> GetJobExecutionStatusSummary(
            DateTimeOffset? startTimeUtc, DateTimeOffset? endTimeUtc = null);
    }
    public class ExecutionLogReadArgs
    {
        public ExecutionLogFilter Filter { get; set; }  = null;
        public PageMetadata PageMetadata { get; set; } = null;
        public long FirstLogId { get; set; } = 0;
    }
}