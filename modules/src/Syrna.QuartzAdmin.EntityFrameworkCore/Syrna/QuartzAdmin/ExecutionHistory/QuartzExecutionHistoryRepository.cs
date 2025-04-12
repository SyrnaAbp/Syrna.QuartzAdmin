using Microsoft.EntityFrameworkCore;
using Syrna.QuartzAdmin.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Syrna.QuartzAdmin.ExecutionHistory;

internal class QuartzExecutionHistoryRepository
    : EfCoreRepository<QuartzAdminDbContext, QuartzExecutionHistory, long>, IQuartzExecutionHistoryRepository
{
    public QuartzExecutionHistoryRepository(IDbContextProvider<QuartzAdminDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<QuartzExecutionHistory> FindByFireInstanceIdAsync(string fireInstanceId, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.FireInstanceId == fireInstanceId, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<QuartzExecutionHistory>> GetLastOfEveryJobAsync(string schedulerName, int limitPerJob, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var sub = query.OrderByDescending(a => a.FireTimeUtc);

        var quartzJobHistories = await query
            .Where(x => x.SchedulerName == schedulerName)
            .Select(x => $"{x.JobGroup}.{x.JobName}")
            .Distinct()
            .SelectMany(a => sub.Where(b => $"{b.JobGroup}.{b.JobName}" == a).Take(limitPerJob), (a, b) => b)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return quartzJobHistories
            .OrderBy(x => $"{x.TriggerGroup}.{x.TriggerName}")
            .ThenBy(x => x.FireTimeUtc)
            .ToList();
    }

    public virtual async Task<List<QuartzExecutionHistory>> GetLastOfEveryTriggerAsync(
        string schedulerName,
        int limitPerTrigger,
        int skipPerTrigger = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var sub = query.OrderByDescending(a => a.FireTimeUtc);

        var quartzJobHistories = await query.Where(x => x.SchedulerName == schedulerName)
            .Select(x => $"{x.TriggerGroup}.{x.TriggerName}")
            .Distinct()
            .SelectMany(a => sub.Where(b => $"{b.TriggerGroup}.{b.TriggerName}" == a).Skip(skipPerTrigger).Take(limitPerTrigger), (a, b) => b)
            .ToListAsync(GetCancellationToken(cancellationToken));

        quartzJobHistories = quartzJobHistories.OrderBy(x => $"{x.TriggerGroup}.{x.TriggerName}").ThenBy(x => x.FireTimeUtc).ToList();

        return quartzJobHistories;
    }

    public virtual async Task<List<QuartzExecutionHistory>> GetLastAsync(string schedulerName, int limit, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var quartzJobHistories = await query
            .Where(x => x.SchedulerName == schedulerName)
            .OrderByDescending(y => y.FireTimeUtc)
            .Take(limit)
            .ToListAsync(GetCancellationToken(cancellationToken));

        quartzJobHistories.Reverse();

        return quartzJobHistories;
    }

    public virtual async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var sub = query.OrderByDescending(a => a.FireTimeUtc);

        query = query
            .OrderByDescending(a => a.FireTimeUtc)
            .Select(x => $"{x.TriggerGroup}.{x.TriggerName}")
            .Distinct()
            .SelectMany(trigger => sub.Where(b => $"{b.TriggerGroup}.{b.TriggerName}" == trigger).Skip(10), (a, b) => b);

        await query.ExecuteDeleteAsync(GetCancellationToken(cancellationToken));
    }
    //
    public async Task<IQueryable<QuartzExecutionHistory>> GetExecutionLogs(ExecutionLogFilter filter = null, long firstLogId = 0)
    {
        var q = await GetQueryableAsync();
        if (filter != null)
        {
            if (filter.JobName != null)
            {
                q = q.Where(l => l.JobName == filter.JobName);
            }

            if (filter.JobGroup != null)
            {
                q = q.Where(l => l.JobGroup == filter.JobGroup);
            }

            if (filter.TriggerName != null)
            {
                q = q.Where(l => l.TriggerName == filter.TriggerName);
            }

            if (filter.TriggerGroup != null)
            {
                q = q.Where(l => l.TriggerGroup == filter.TriggerGroup);
            }

            if (filter.LogTypes != null && filter.LogTypes.Any())
            {
                q = q.Where(l => filter.LogTypes.Contains(l.LogType));
            }

            if (filter.DateAddedStartUtc != null)
            {
                q = q.Where(l => l.DateAddedUtc >= filter.DateAddedStartUtc);
            }

            if (filter.DateAddedEndUtc != null)
            {
                q = q.Where(l => l.DateAddedUtc < filter.DateAddedEndUtc);
            }

            if (filter.ErrorOnly)
            {
                q = q.Where(l => (l.IsException ?? false) || l.IsSuccess.HasValue && !l.IsSuccess.Value);
            }

            if (filter.MessageContains != null)
            {
                var likeStr = $"%{filter.MessageContains}%";
                q = q.Where(l => EF.Functions.Like(l.JobName ?? string.Empty, likeStr)
                    || EF.Functions.Like(l.TriggerName ?? string.Empty, likeStr)
                    || EF.Functions.Like(l.Result ?? string.Empty, likeStr)
                    || EF.Functions.Like(l.ErrorMessage ?? string.Empty, likeStr)
                    || l.ExecutionHistoryDetail != null
                        && (EF.Functions.Like(
                            l.ExecutionHistoryDetail.ExecutionDetails ?? string.Empty, likeStr)
                            || EF.Functions.Like(l.ExecutionHistoryDetail.ErrorStackTrace ?? string.Empty, likeStr)
                            || l.ExecutionHistoryDetail.ErrorCode != null && l.ExecutionHistoryDetail.ErrorCode.Value.ToString() == filter.MessageContains));
            }

            if (!filter.IncludeSystemJobs)
            {
                q = q.Where(l => !(l.TriggerGroup == Constants.SYSTEM_GROUP ||
                    l.JobGroup == Constants.SYSTEM_GROUP));
            }
        }

        IOrderedQueryable<QuartzExecutionHistory> ordered;
        if (filter != null && filter.IsAscending)
        {
            ordered = q.OrderBy(l => l.DateAddedUtc).ThenBy(l => l.FireTimeUtc);
        }
        else
        {
            if (firstLogId > 0)
            {
                // to avoid incorrect page data for descing order
                q = q.Where(l => l.Id <= firstLogId);
            }
            ordered = q.OrderByDescending(l => l.DateAddedUtc).ThenByDescending(l => l.FireTimeUtc);
        }

        return ordered;
    }

    public async Task<IList<string>> GetJobGroups()
    {
        var query = await GetQueryableAsync();
        return query
            .Where(l => l.LogType != LogType.System)
            .Select(l => l.JobGroup ?? string.Empty)
            .Distinct().OrderBy(l => l)
            .ToList();
    }

    public async Task<IList<string>> GetJobNames()
    {
        var query = await GetQueryableAsync();
        return query
            .Where(l => l.LogType != LogType.System)
            .Select(l => l.JobName ?? string.Empty)
            .Distinct().OrderBy(l => l)
            .ToList();
    }

    public async Task<IQueryable<QuartzExecutionHistory>> GetLatestExecutionLog(string jobName, string jobGroup, string triggerName, string triggerGroup, long firstLogId = 0, HashSet<LogType> logTypes = null)
    {
        var query = await GetQueryableAsync();
        var q = query.Where(l => l.JobName == jobName &&
          l.JobGroup == jobGroup);

        if (triggerName is not null)
        {
            q = q.Where(l => l.TriggerName == triggerName &&
                l.TriggerGroup == triggerGroup);
        }

        if (firstLogId > 0)
        {
            // to avoid incorrect page data
            q = q.Where(l => l.Id <= firstLogId);
        }

        if (logTypes != null)
        {
            q = q = q.Where(l => logTypes.Contains(l.LogType));
        }

        var ordered = q.OrderByDescending(l => l.DateAddedUtc).ThenByDescending(l => l.FireTimeUtc);
        return ordered;
    }

    public async Task<IList<string>> GetTriggerGroups()
    {
        var query = await GetQueryableAsync();
        return query
            .Where(l => l.LogType != LogType.System)
            .Select(l => l.TriggerGroup ?? string.Empty)
            .Distinct().OrderBy(l => l)
            .ToList();
    }

    public async Task<IList<string>> GetTriggerNames()
    {
        var query = await GetQueryableAsync();
        return query
            .Where(l => l.LogType != LogType.System)
            .Select(l => l.TriggerName ?? string.Empty)
            .Distinct().OrderBy(l => l)
            .ToList();
    }

    public async Task<JobExecutionStatusSummaryModel> GetJobExecutionStatusSummary(DateTimeOffset? startTimeUtc, DateTimeOffset? endTimeUtc = null)
    {
        var query = await GetQueryableAsync();
        var q = query.Where(l => l.LogType == LogType.ScheduleJob);
        if (startTimeUtc.HasValue)
        {
            q = q.Where(l => l.DateAddedUtc >= startTimeUtc.Value);
        }
        if (endTimeUtc.HasValue)
        {
            q = q.Where(l => l.DateAddedUtc < endTimeUtc.Value);
        }

        var statusList = q.Select(l => new
        {
            l.DateAddedUtc,
            ExecutionStatus = l.IsException ?? false ?
                // has exception
                JobExecutionStatus.Failed :
                // vetoed?
                l.IsVetoed ?? false ?
                    JobExecutionStatus.Vetoed :
                    // is success null?
                    l.IsSuccess.HasValue ?
                        l.IsSuccess.Value ? JobExecutionStatus.Success : JobExecutionStatus.Failed :
                        JobExecutionStatus.Executing
        });

        var statusGroup = await statusList.GroupBy(l => l.ExecutionStatus)
            .Select(g => new
            {
                EarliestDateAdded = g.Min(l => l.DateAddedUtc),
                ExecutionStatus = g.Key,
                Count = g.Count()
            }).ToListAsync();

        if (!statusGroup.Any())
            return new();

        return new JobExecutionStatusSummaryModel
        {
            StartDateTimeUtc = statusGroup.Min(s => s.EarliestDateAdded).DateTime,
            Data = statusGroup.Select(s => new KeyValuePair<JobExecutionStatus, int>(s.ExecutionStatus, s.Count))
                    .ToList()
        };
    }

    public async Task MarkExecutingJobAsIncomplete(CancellationToken cancellToken = default)
    {
        var isSuccessNullJobs = (await GetQueryableAsync()).Where(l => !l.IsSuccess.HasValue &&
            l.LogType == LogType.ScheduleJob);

        foreach (var log in isSuccessNullJobs)
        {
            log.IsSuccess = false;
            log.ErrorMessage = "Incomplete execution.";
            log.JobRunTime = null;
        }

        await SaveChangesAsync(cancellToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<QuartzExecutionHistory, bool>> predicate)
    {
        var query = await GetQueryableAsync();
        return await query.Where(predicate).AnyAsync();
    }

    public async Task<QuartzExecutionHistory> FirstOrDefaultAsync(Expression<Func<QuartzExecutionHistory, bool>> predicate)
    {
        var query = await GetQueryableAsync();
        return await query.Where(predicate).FirstOrDefaultAsync();
    }

    public async Task<int> DeleteLogsByDays(int daysToKeep, CancellationToken cancelToken = default)
    {
        DateTime oldDate = DateTime.UtcNow.Date.AddDays(-(daysToKeep + 1));
        var query = await GetQueryableAsync();
        var list = query.Where(w => w.DateAddedUtc < oldDate).ToList();
        await DeleteManyAsync(list, true, cancelToken);
        var deleted = list.Count - await query.CountAsync(w => w.DateAddedUtc < oldDate);
        return deleted;
    }

    public async new Task SaveChangesAsync(CancellationToken cancelToken = default)
    {
        await base.SaveChangesAsync(cancelToken);
    }
}
