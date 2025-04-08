using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminDomainSharedModule)
        )]
    public class QuartzAdminDomainModule : AbpModule
    {

    }
}
