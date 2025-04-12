using Syrna.BlazoriseQuartz.ExecutionLog.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.ExecutionLog
{
    public interface IExecutionLogAppService
    {
        Task<PagedList<ExecutionLogDto>> GetLatestExecutionLog(string jobName, string jobGroup,
            string triggerName, string triggerGroup,
            PageMetadata pageMetadata = null, long firstLogId = 0,
            HashSet<LogType> logTypes = null);
        Task<PagedList<ExecutionLogDto>> GetExecutionLogs(
            ExecutionLogFilter filter = null,
            PageMetadata pageMetadata = null, long firstLogId = 0);
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
}