using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.MainDemo.Data;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Syrna.QuartzAdmin.MainDemo.PostgreSql.EntityFrameworkCore;

public class EntityFrameworkCoreMainDemoDbSchemaMigrator(
    IServiceProvider serviceProvider)
        : IMainDemoDbSchemaMigrator, ITransientDependency
{
    public async Task MigrateAsync()
    {
        /* We intentionally resolving the LayoutMigrationsDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await serviceProvider
            .GetRequiredService<MainDemoMigrationsDbContext>()
            .Database
            .MigrateAsync();
    }
}