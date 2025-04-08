using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Authorization;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminDomainSharedModule),
        typeof(AbpDddApplicationContractsModule),
        typeof(AbpAuthorizationModule)
        )]
    public class QuartzAdminApplicationContractsModule : AbpModule
    {

    }
}
