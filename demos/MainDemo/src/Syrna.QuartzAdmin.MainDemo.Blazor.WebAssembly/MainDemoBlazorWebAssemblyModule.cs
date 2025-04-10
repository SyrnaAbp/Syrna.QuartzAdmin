using Syrna.QuartzAdmin.MainDemo.Blazor;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.WebAssembly;

[DependsOn(    typeof(MainDemoBlazorModule))]
[DependsOn(    typeof(MainDemoHttpApiClientModule))]
public class MainDemoBlazorWebAssemblyModule : AbpModule
{
}
