using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [DisallowConcurrentExecution]
    [QuartzTrigger(5, 0, 0, Desciption = "Automatic job of welcome information")]
    public class AutoJob1 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello from Auto Job1 {DateTime.Now}");

            context.SetIsSuccess(true);
            context.SetReturnCode(0);
            context.SetExecutionDetails("Executed successfully");

            context.Result = $"Hello from Auto Job1 {DateTime.Now}";
            //context.JobDetail.JobDataMap[JobDataMapKeys.IsSuccess] = true;
            //context.JobDetail.JobDataMap[JobDataMapKeys.ReturnCode] = 0;
            //context.JobDetail.JobDataMap[JobDataMapKeys.ExecutionDetails] = "Executed successfully";
            return Task.CompletedTask;
        }
    }
}
