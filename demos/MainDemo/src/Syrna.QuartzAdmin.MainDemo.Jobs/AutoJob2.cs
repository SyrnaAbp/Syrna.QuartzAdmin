using Microsoft.Extensions.Logging;
using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System.Threading;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [QuartzTrigger(2,0, "this is an long job test", "_longjobauto")]
    public class AutoJob2(ILogger<HelloJob> logger) : IJob
    {
        private async Task ExecuteJob(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"Hello from AutoJob {DateTime.Now}");

            context.SetExecutionDetails("Executing first delay 30s");
            logger.LogInformation("Delaying {delay} sec", 30);
            await Task.Delay(30000);

            context.SetExecutionDetails("Executing second delay 40s");
            logger.LogInformation("Delaying {delay} sec", 40);
            await Task.Delay(40000);

            context.SetExecutionDetails("Executing third delay 50s");
            logger.LogInformation("Delaying {delay} sec", 50);
            await Task.Delay(50000);

            context.SetExecutionDetails("Executing third delay 60s");
            logger.LogInformation("Delaying {delay} sec", 60);
            await Task.Delay(60000);

            context.SetIsSuccess(true);
            context.SetReturnCode(0);
            context.SetExecutionDetails("Executed successfully");
            context.Result = $"Hello from AutoJob {DateTime.Now}";

            await Task.CompletedTask;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var taskCompletionSource = new TaskCompletionSource();
            context.CancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                // ReSharper disable once InvertIf
                taskCompletionSource.TrySetCanceled();
            });
            var completedTask = await Task.WhenAny(ExecuteJob(context), taskCompletionSource.Task);

            await completedTask;
        }
    }
}
