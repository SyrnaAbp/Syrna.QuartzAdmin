using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminHttpApiClientModule),
        typeof(AbpHttpClientIdentityModelModule)
        )]
    public class QuartzAdminConsoleApiClientModule : AbpModule
    {
        
    }
}
