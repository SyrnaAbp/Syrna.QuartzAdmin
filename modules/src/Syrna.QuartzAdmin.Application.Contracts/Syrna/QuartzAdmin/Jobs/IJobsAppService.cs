using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.Jobs
{
    public interface IJobsAppService : IApplicationService
    {
        Task<ApiResponse> CreateJobConfiguration(JobDetails model);
        Task<bool> DeleteJob(string jobGroup, string jobName);
        Task<List<JobListDetail>> GetAllJobs();
        Task<IJobDetail> GetJobConfiguration(string jobGroup, string jobName);
        Task<bool> InterruptJob(string fireInstanceId);
        Task<ApiResponse> TriggerJob(string jobGroup, string jobName);
        Task<ApiResponse> UpdateJobConfiguration(JobDetails model);
    }
}