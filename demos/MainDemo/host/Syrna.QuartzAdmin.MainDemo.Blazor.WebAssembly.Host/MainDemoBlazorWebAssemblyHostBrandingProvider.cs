using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.WebAssembly.Host;

[Dependency(ReplaceServices = true)]
public class MainDemoBlazorWebAssemblyHostBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Quartz Admin";
    public override string LogoUrl => "logo.svg";
}
