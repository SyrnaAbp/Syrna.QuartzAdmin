using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.ExecutionHistory;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.EntityFrameworkCore
{
    [DependsOn(typeof(QuartzAdminDomainModule))]
    [DependsOn(typeof(AbpEntityFrameworkCoreModule))]
    public class QuartzAdminEntityFrameworkCoreModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            //QuartzAdminEfCoreEntityExtensionMappings.Configure();
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<QuartzAdminDbContext>(options =>
            {
                options.AddRepository<QuartzExecutionHistory, QuartzExecutionHistoryRepository>();
                options.AddRepository<QuartzJobSummary, QuartzJobSummaryRepository>();
                options.AddDefaultRepositories(includeAllEntities: true);
            });
        }
    }
}
