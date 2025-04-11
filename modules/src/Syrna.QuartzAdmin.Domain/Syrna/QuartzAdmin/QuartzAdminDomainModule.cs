using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Syrna.QuartzAdmin.ExecutionHistory;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin
{
    [DependsOn(typeof(QuartzAdminDomainSharedModule))]
    //[DependsOn(typeof(AbpQuartzModule))]
    public class QuartzAdminDomainModule : AbpModule
    {

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var quartzOptions = configuration.GetSection("Quartz");
            if (quartzOptions == null)
            {
                if (quartzOptions["Enabled"] == "true") { }
                var scheduler = context.ServiceProvider.GetRequiredService<IScheduler>();
                var executionHistoryStore = context.ServiceProvider.GetRequiredService<AbpExecutionHistoryStore>();
                scheduler.Context.SetExecutionHistoryStore(executionHistoryStore);
            }
        }
    }

    //public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    //{
    //    await base.OnApplicationInitializationAsync(context);

    //    await context.AddBackgroundWorkerAsync<ExecutionHistoryCleanBackgroundWorker>();
    //}
}
