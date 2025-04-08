using Volo.Abp.AspNetCore.Components.Server.Theming;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.Blazor.Server
{
    [DependsOn(
        typeof(AbpAspNetCoreComponentsServerThemingModule),
        typeof(QuartzAdminBlazorModule)
        )]
    public class QuartzAdminBlazorServerModule : AbpModule
    {
        
    }
}