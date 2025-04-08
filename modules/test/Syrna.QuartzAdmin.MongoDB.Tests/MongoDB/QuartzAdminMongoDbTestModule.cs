using System;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.MongoDB
{
    [DependsOn(
        typeof(QuartzAdminTestBaseModule),
        typeof(QuartzAdminMongoDbModule)
        )]
    public class QuartzAdminMongoDbTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpDbConnectionOptions>(options =>
            {
                options.ConnectionStrings.Default = MongoDbFixture.GetRandomConnectionString();
            });
        }
    }
}