using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.ExecutionHistory
{
    public interface IExecutionHistoryStore
    {
        string SchedulerName { get; set; }

        Task<ExecutionHistoryEntry> Get(string fireInstanceId);
        Task Save(ExecutionHistoryEntry entry);
        Task Purge();

        Task<IEnumerable<ExecutionHistoryEntry>> FilterLastOfEveryJob(int limitPerJob);
        Task<IEnumerable<ExecutionHistoryEntry>> FilterLastOfEveryTrigger(int limitPerTrigger);
        Task<IEnumerable<ExecutionHistoryEntry>> FilterLast(int limit);

        Task<int> GetTotalJobsExecuted();
        Task<int> GetTotalJobsFailed();

        Task IncrementTotalJobsExecuted();
        Task IncrementTotalJobsFailed();

        Task<bool> ExistsAsync(QuartzExecutionHistory log);
        bool Exists(QuartzExecutionHistory log);
        Task<int> DeleteLogsByDays(int daysToKeep, CancellationToken cancelToken = default);
        Task AddExecutionLog(QuartzExecutionHistory log, CancellationToken cancelToken = default);
        ValueTask UpdateExecutionLog(QuartzExecutionHistory log);
        Task SaveChangesAsync(CancellationToken cancelToken = default);
        Task MarkExecutingJobAsIncomplete(CancellationToken cancellationToken = default);
    }
}
