using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [QuartzTrigger(5, "this e sq test", "_hellojobauto")]
    public class AutoJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello from AutoJob {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
