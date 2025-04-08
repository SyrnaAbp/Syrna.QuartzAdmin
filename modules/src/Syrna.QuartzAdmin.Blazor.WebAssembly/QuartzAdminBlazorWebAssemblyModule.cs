using Volo.Abp.AspNetCore.Components.WebAssembly.Theming;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.Blazor.WebAssembly
{
    [DependsOn(
        typeof(QuartzAdminBlazorModule),
        typeof(QuartzAdminHttpApiClientModule),
        typeof(AbpAspNetCoreComponentsWebAssemblyThemingModule)
        )]
    public class QuartzAdminBlazorWebAssemblyModule : AbpModule
    {
        
    }
}