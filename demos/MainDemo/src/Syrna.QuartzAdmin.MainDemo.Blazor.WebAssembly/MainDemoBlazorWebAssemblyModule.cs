using Syrna.QuartzAdmin.MainDemo.Blazor;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.WebAssembly;

[DependsOn(
    typeof(MainDemoBlazorModule)
)]
public class MainDemoBlazorWebAssemblyModule : AbpModule
{
}
