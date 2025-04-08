using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Server;

[DependsOn(
    typeof(MainDemoBlazorModule)
)]
public class MainDemoBlazorServerModule : AbpModule
{

}
