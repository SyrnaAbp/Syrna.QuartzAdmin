using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminApplicationModule),
        typeof(QuartzAdminDomainTestModule)
        )]
    public class QuartzAdminApplicationTestModule : AbpModule
    {

    }
}
