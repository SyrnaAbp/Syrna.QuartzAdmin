using Syrna.QuartzAdmin.ExecutionHistory;
using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.ExecutionLog
{
    public class ExecutionLogAppService(IQuartzExecutionHistoryRepository executionLogRepository) : QuartzAdminAppService, IExecutionLogAppService
    {
        public async Task<DataEnvelope<ExecutionLogDto>> GetLatestExecutionLog(string jobName, string jobGroup, string triggerName, string triggerGroup, PageMetadata pageMetadata = null, long firstLogId = 0, HashSet<LogType> logTypes = null)
        {
            var query = await executionLogRepository.GetLatestExecutionLog(jobName, jobGroup, triggerName, triggerGroup, firstLogId, logTypes);
            var totalRecords = query.Count();
            if (pageMetadata == null)
            {
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. query]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = 0 };
            }
            else
            {
                PageMetadata newPageMetadata = pageMetadata;
                if (pageMetadata.Page == 0)
                {
                    newPageMetadata = new PageMetadata { TotalCount = totalRecords };
                }

                var result = query
                    .Skip(pageMetadata.Page * pageMetadata.PageSize)
                    .Take(pageMetadata.PageSize)
                    .ToList();
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. result]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = totalRecords };
            }
        }

        public async Task<DataEnvelope<ExecutionLogDto>> GetExecutionLogs(ExecutionLogFilter filter = null, PageMetadata pageMetadata = null, long firstLogId = 0)
        {
            var query = await executionLogRepository.GetExecutionLogs(filter, firstLogId);
            var totalRecords = query.Count();
            if (pageMetadata == null)
            {
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. query]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = totalRecords };
            }
            else
            {
                PageMetadata newPageMetadata = pageMetadata;
                if (pageMetadata.Page == 0)
                {
                    // if first page, get the total records
                    newPageMetadata = new PageMetadata { TotalCount = totalRecords };
                }

                var result = query
                    .Skip(pageMetadata.Page * pageMetadata.PageSize)
                    .Take(pageMetadata.PageSize)
                    .ToList();
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. result]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = totalRecords };
            }
        }

        public async Task<IList<string>> GetJobNames()
        {
            return await executionLogRepository.GetJobNames();
        }

        public async Task<IList<string>> GetJobGroups()
        {
            return await executionLogRepository.GetJobGroups();
        }

        public async Task<IList<string>> GetTriggerNames()
        {
            return await executionLogRepository.GetTriggerNames();
        }

        public async Task<IList<string>> GetTriggerGroups()
        {
            return await executionLogRepository.GetTriggerGroups();
        }


        public async Task<JobExecutionStatusSummaryModel> GetJobExecutionStatusSummary(DateTimeOffset? startTimeUtc, DateTimeOffset? endTimeUtc = null)
        {
            return await executionLogRepository.GetJobExecutionStatusSummary(startTimeUtc, endTimeUtc);
        }
    }
}

