using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [QuartzTrigger(5, "this e sq test", "_hellojobauto")]
    public class AutoJob : IJob
    {
        public async Task CanFireIt()
        {
            Random random = new();
            int randomNumber = random.Next(1, 100);
            if (randomNumber % 2 == 0)
            {
                throw new Exception("Test exception");
            }
            await Task.CompletedTask;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello from AutoJob {DateTime.Now}");

            await CanFireIt();

            context.SetIsSuccess(true);
            context.SetReturnCode(0);
            context.SetExecutionDetails("Executed successfully");
            context.Result = $"Hello from AutoJob {DateTime.Now}";

            await Task.CompletedTask;
        }
    }
}
