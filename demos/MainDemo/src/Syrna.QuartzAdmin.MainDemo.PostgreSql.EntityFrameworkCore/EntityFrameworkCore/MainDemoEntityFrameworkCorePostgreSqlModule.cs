using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore;
using Syrna.QuartzAdmin.MainDemo.PostgreSql.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Syrna.Alpha.SilkierQuartz.PostgreSql.EntityFrameworkCore;

[DependsOn(typeof(MainDemoEntityFrameworkCoreModule))]
public class MainDemoEntityFrameworkCorePostgreSqlModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MainDemoMigrationsDbContext>();
    }
}