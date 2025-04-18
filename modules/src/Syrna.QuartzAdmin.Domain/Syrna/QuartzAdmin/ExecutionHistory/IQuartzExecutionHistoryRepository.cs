using System;
using System.Threading.Tasks;
using System.Threading;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Syrna.QuartzAdmin.ExecutionHistory;

public interface IQuartzExecutionHistoryRepository : IBasicRepository<QuartzExecutionHistory, long>
{
    Task<QuartzExecutionHistory> FindByFireInstanceIdAsync(string fireInstanceId, CancellationToken cancellationToken = default);

    Task<List<QuartzExecutionHistory>> GetLastOfEveryJobAsync(string schedulerName, int limitPerJob, CancellationToken cancellationToken = default);

    Task<List<QuartzExecutionHistory>> GetLastOfEveryTriggerAsync(
        string schedulerName,
        int limitPerTrigger,
        int skipPerTrigger = 0,
        CancellationToken cancellationToken = default);

    Task<List<QuartzExecutionHistory>> GetLastAsync(string schedulerName, int limit, CancellationToken cancellationToken = default);

    Task PurgeAsync(CancellationToken cancellationToken = default);

    //
    Task<IQueryable<QuartzExecutionHistory>> GetLatestExecutionLog(string jobName, string jobGroup, string triggerName, string triggerGroup, long firstLogId = 0, HashSet<LogType> logTypes = null);
    Task<IQueryable<QuartzExecutionHistory>> GetExecutionLogs(ExecutionLogFilter filter = null, long firstLogId = 0);
    Task<IList<string>> GetJobNames();
    Task<IList<string>> GetJobGroups();
    Task<IList<string>> GetTriggerNames();
    Task<IList<string>> GetTriggerGroups();
    Task<JobExecutionStatusSummaryModel> GetJobExecutionStatusSummary(DateTimeOffset? startTimeUtc, DateTimeOffset? endTimeUtc = null);
    Task MarkExecutingJobAsIncomplete(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<QuartzExecutionHistory, bool>> predicate);
    Task<QuartzExecutionHistory> FirstOrDefaultAsync(Expression<Func<QuartzExecutionHistory, bool>> predicate);
    Task<int> DeleteLogsByDays(int daysToKeep, CancellationToken cancelToken = default);
    Task SaveChangesAsync(CancellationToken cancelToken = default);

}
