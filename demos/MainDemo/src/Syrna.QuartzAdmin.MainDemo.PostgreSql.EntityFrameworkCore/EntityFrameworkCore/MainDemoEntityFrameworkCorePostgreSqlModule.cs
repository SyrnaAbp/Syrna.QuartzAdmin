using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore;
using Syrna.QuartzAdmin.MainDemo.PostgreSql.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Syrna.Alpha.SilkierQuartz.PostgreSql.EntityFrameworkCore;

[DependsOn(typeof(MainDemoEntityFrameworkCoreModule))]
public class MainDemoEntityFrameworkCorePostgreSqlModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MainDemoMigrationsDbContext>(options =>
        {
            /* Remove "includeAllEntities: true" to create
             * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also SilkierQuartzDemoMigrationsDbContextFactory for EF Core tooling. */
            options.UseNpgsql();
        });
    }
}