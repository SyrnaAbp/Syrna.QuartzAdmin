using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.Scheduler
{
    public interface ISchedulerAppService : IApplicationService
    {
        Task<ApiResponse> ClearScheduler();
        Task<List<ExecutingJobContext>> GetCurrentExecutingJobs();
        Task<SchedulerDetails> GetSchedulerMetaData();
        Task<ApiResponse> PauseAll();
        Task<ApiResponse> PauseAllJobsInGroup(string groupName);
        Task<ApiResponse> PauseAllTriggersInGroup(string groupName);
        Task<ApiResponse> PauseScheduler();
        Task<ApiResponse> ResumeAll();
        Task<ApiResponse> ResumeAllJobsInGroup(string groupName);
        Task<ApiResponse> ResumeAllTriggersInGroup(string groupName);
        Task<ApiResponse> ShutDownScheduler(bool waitForJobsToComplete = false);
        Task<ApiResponse> StartScheduler(int? delayMilliseconds = null);
    }
}