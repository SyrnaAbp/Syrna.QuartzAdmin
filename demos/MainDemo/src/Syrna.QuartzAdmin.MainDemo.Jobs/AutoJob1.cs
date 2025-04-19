using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [DisallowConcurrentExecution]
    [QuartzTrigger(5, 0, 0, Description = "Automatic job of welcome information")]
    public class AutoJob1 : IJob
    {
        private Task ExecuteJob(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"Hello from Auto Job1 {DateTime.Now}");

            context.SetIsSuccess(true);
            context.SetReturnCode(0);
            context.SetExecutionDetails("Executed successfully");

            context.Result = $"Hello from Auto Job1 {DateTime.Now}";
            return Task.CompletedTask;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var taskCompletionSource = new TaskCompletionSource();
            context.CancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });
            var completedTask = await Task.WhenAny(ExecuteJob(context), taskCompletionSource.Task);

            await completedTask;
        }
    }
}
