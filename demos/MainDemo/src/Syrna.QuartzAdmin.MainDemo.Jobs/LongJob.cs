using Microsoft.Extensions.Logging;
using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.MainDemo.Jobs;

public class LongJob(
    ILogger<LongJob> logger,
    IDataMapValueResolver dmvResolver)
    : IJob
{
    private const string PropertyMessage = "message";
    private const string PropertyDelayInMs = "delay";

    private async Task ExecuteJob(IJobExecutionContext context)
    {
        var taskCompletionSource = new TaskCompletionSource();
        context.CancellationToken.Register(() =>
        {
            // We received a cancellation message, cancel the TaskCompletionSource.Task
            taskCompletionSource.TrySetCanceled();
        });

        var task = Task.Run(async () =>
        {
            var rawMsg = context.GetDataMapValue(PropertyMessage);

            // resolve dynamic variable
            var msg = dmvResolver.Resolve(rawMsg);

            logger.LogInformation("Hello! {message}", msg);

            if (context.MergedJobDataMap.TryGetIntValueFromString(PropertyDelayInMs, out var delay)
                && delay > 0)
            {
                logger.LogInformation("Delaying {delay} ms", delay);
                await Task.Delay(delay, context.CancellationToken);
            }

            // Write the output to display in execution log
            context.Result = $"Hello! {msg}";
            context.JobDetail.JobDataMap[JobDataMapKeys.IsSuccess] = true;
            context.JobDetail.JobDataMap[JobDataMapKeys.ReturnCode] = 0;
            context.JobDetail.JobDataMap[JobDataMapKeys.ExecutionDetails] = "Executed successfully";
        });
        var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);

        await completedTask;
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

