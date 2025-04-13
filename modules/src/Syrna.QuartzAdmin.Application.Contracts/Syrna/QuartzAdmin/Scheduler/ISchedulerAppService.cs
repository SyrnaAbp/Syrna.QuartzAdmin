using Quartz;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Triggers;
using System.Collections.Generic;
using System.Threading;
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
        Task<ApiResponse> ShutDownSchedulerExt(bool waitForJobsToComplete = false);
        Task<ApiResponse> StartSchedulerExt(int? delayMilliseconds = null);

        Task<ScheduleModel> GetScheduleModelAsync(ITrigger trigger);
        IAsyncEnumerable<ScheduleModel> GetAllJobsAsync(ScheduleJobFilter filter = null);
        Task CreateSchedule(JobDetailModel jobDetailModel, TriggerDetailModel triggerDetailModel);
        Task<IReadOnlyCollection<string>> GetJobGroups();
        Task<IReadOnlyCollection<string>> GetTriggerGroups();
        Task<JobDetailModel> GetJobDetail(string jobName, string groupName);
        Task<TriggerDetailModel> GetTriggerDetail(string triggerName, string triggerGroup);
        Task<bool> ContainsTriggerKey(string triggerName, string triggerGroup);
        Task<bool> ContainsJobKey(string jobName, string jobGroup);
        Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancelToken = default);
        Task PauseTrigger(string triggerName, string triggerGroup);
        Task ResumeTrigger(string triggerName, string triggerGroup);
        Task TriggerJob(string jobName, string jobGroup);
        Task<bool> DeleteSchedule(ScheduleModel model);
        Task UpdateSchedule(Key oldJobKey, Key oldTriggerKey,
            JobDetailModel newJobModel, TriggerDetailModel newTriggerModel);
        Task<SchedulerMetaData> GetMetadataAsync();
        Task<IList<KeyValuePair<string, int>>> GetScheduledJobSummary();
        Task PauseAllSchedules();
        Task ResumeAllSchedules();
        Task ShutdownScheduler();
        Task StartScheduler();
        Task StandbyScheduler();

    }
}