using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [QuartzTrigger(1, 0, "this is an job test", "_jobauto")]
    public class AutoJob : IJob
    {
        private static async Task CanFireIt()
        {
            Random random = new();
            var randomNumber = random.Next(1, 100);
            if (randomNumber % 2 == 0)
            {
                throw new Exception("Test exception");
            }
            await Task.CompletedTask;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
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
