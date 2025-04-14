using Microsoft.Extensions.Logging;
using Quartz;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System.Threading;

namespace Syrna.QuartzAdmin.MainDemo.Jobs
{
    [QuartzTrigger(120, "this is an long job test", "_longjobauto")]
    public class AutoJob2 : IJob
    {
        private readonly ILogger<HelloJob> _logger;

        public AutoJob2(ILogger<HelloJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello from AutoJob {DateTime.Now}");
            context.CancellationToken.ThrowIfCancellationRequested();

            context.SetExecutionDetails("Executing first delay 30s");
            _logger.LogInformation("Delaying {delay} sec", 30);
            await Task.Delay(30000);

            context.SetExecutionDetails("Executing second delay 40s");
            _logger.LogInformation("Delaying {delay} sec", 40);
            await Task.Delay(40000);

            context.SetExecutionDetails("Executing third delay 50s");
            _logger.LogInformation("Delaying {delay} sec", 50);
            await Task.Delay(50000);

            context.SetExecutionDetails("Executing third delay 60s");
            _logger.LogInformation("Delaying {delay} sec", 60);
            await Task.Delay(60000);

            context.SetIsSuccess(true);
            context.SetReturnCode(0);
            context.SetExecutionDetails("Executed successfully");
            context.Result = $"Hello from AutoJob {DateTime.Now}";

            await Task.CompletedTask;
        }
    }
}
