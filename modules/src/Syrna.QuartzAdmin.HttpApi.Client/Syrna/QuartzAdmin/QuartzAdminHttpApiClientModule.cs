using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminApplicationContractsModule),
        typeof(AbpHttpClientModule))]
    public class QuartzAdminHttpApiClientModule : AbpModule
    {
        public const string RemoteServiceName = QuartzAdminRemoteServiceConsts.RemoteServiceName;

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddHttpClientProxies(
                typeof(QuartzAdminApplicationContractsModule).Assembly,
                RemoteServiceName
            );
            
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<QuartzAdminApplicationContractsModule>();
            });
        }
    }
}
