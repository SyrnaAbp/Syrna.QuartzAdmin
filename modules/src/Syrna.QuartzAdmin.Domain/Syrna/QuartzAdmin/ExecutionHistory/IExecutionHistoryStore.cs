using System.Collections.Generic;
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
    }
}
