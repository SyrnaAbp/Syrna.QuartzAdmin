using Microsoft.AspNetCore.Mvc;
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
        [HttpPost]
        public async Task<DataEnvelope<ExecutionLogDto>> GetLatestExecutionLog(LatestExecutionLogReadArgs args)
        {
            var query = await executionLogRepository.GetLatestExecutionLog(args.JobName, args.JobGroup, args.TriggerName, args.TriggerGroup, args.FirstLogId, args.LogTypes);
            var totalRecords = query.Count();
            if (args.PageMetadata == null)
            {
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. query]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = 0 };
            }
            else
            {
                var newPageMetadata = args.PageMetadata;
                if (args.PageMetadata.Page == 0)
                {
                    newPageMetadata = new PageMetadata { TotalCount = totalRecords };
                }

                var result = query
                    .Skip(args.PageMetadata.Page * args.PageMetadata.PageSize)
                    .Take(args.PageMetadata.PageSize)
                    .ToList();
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. result]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = totalRecords };
            }
        }

        [HttpPost]
        public async Task<DataEnvelope<ExecutionLogDto>> GetExecutionLogs(ExecutionLogReadArgs args)
        {
            var query = await executionLogRepository.GetExecutionLogs(args.Filter, args.FirstLogId);
            var totalRecords = query.Count();
            if (args.PageMetadata == null)
            {
                var list = ObjectMapper.Map<List<QuartzExecutionHistory>, List<ExecutionLogDto>>([.. query]);
                return new DataEnvelope<ExecutionLogDto>() { Items = list, TotalCount = totalRecords };
            }
            else
            {
                var newPageMetadata = args.PageMetadata;
                if (args.PageMetadata.Page == 0)
                {
                    // if first page, get the total records
                    newPageMetadata = new PageMetadata { TotalCount = totalRecords };
                }

                var result = query
                    .Skip(args.PageMetadata.Page * args.PageMetadata.PageSize)
                    .Take(args.PageMetadata.PageSize)
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


        [HttpPost]
        public async Task<JobExecutionStatusSummaryModel> GetJobExecutionStatusSummary(JobExecutionStatusSummaryReadArgs args)
        {
            return await executionLogRepository.GetJobExecutionStatusSummary(args.StartTimeUtc, args.EndTimeUtc);
        }
    }
}

