using Syrna.BlazoriseQuartz.ExecutionLog.Dtos;
using Syrna.QuartzAdmin.ExecutionHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.ExecutionLog
{
    public class ExecutionLogAppService(IQuartzExecutionHistoryRepository executionLogRepository) : QuartzAdminAppService, IExecutionLogAppService
    {
        public async Task<PagedList<ExecutionLogDto>> GetLatestExecutionLog(string jobName, string jobGroup, string triggerName, string triggerGroup, PageMetadata pageMetadata = null, long firstLogId = 0, HashSet<LogType> logTypes = null)
        {
            var query = await executionLogRepository.GetLatestExecutionLog(jobName, jobGroup, triggerName, triggerGroup, firstLogId, logTypes);
            if (pageMetadata == null)
            {
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. query]);
                return new PagedList<ExecutionLogDto>(list);
            }
            else
            {
                PageMetadata newPageMetadata = pageMetadata;
                if (pageMetadata.Page == 0)
                {
                    // if first page, get the total records
                    var totalRecords = query.Count();
                    newPageMetadata = pageMetadata with { TotalCount = totalRecords };
                }

                var result = query
                    .Skip(pageMetadata.Page * pageMetadata.PageSize)
                    .Take(pageMetadata.PageSize)
                    .ToList();
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. result]);
                return new PagedList<ExecutionLogDto>(list, newPageMetadata);
            }
        }

        public async Task<PagedList<ExecutionLogDto>> GetExecutionLogs(ExecutionLogFilter filter = null, PageMetadata pageMetadata = null, long firstLogId = 0)
        {
            var query = await executionLogRepository.GetExecutionLogs(filter, firstLogId);
            if (pageMetadata == null)
            {
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. query]);
                return new PagedList<ExecutionLogDto>(list);
            }
            else
            {
                PageMetadata newPageMetadata = pageMetadata;
                if (pageMetadata.Page == 0)
                {
                    // if first page, get the total records
                    var totalRecords = query.Count();
                    newPageMetadata = pageMetadata with { TotalCount = totalRecords };
                }

                var result = query
                    .Skip(pageMetadata.Page * pageMetadata.PageSize)
                    .Take(pageMetadata.PageSize)
                    .ToList();
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. result]);
                return new PagedList<ExecutionLogDto>(list, newPageMetadata);
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

