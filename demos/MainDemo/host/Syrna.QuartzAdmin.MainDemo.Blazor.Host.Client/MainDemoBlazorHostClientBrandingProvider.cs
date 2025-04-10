using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Host.Client;

[Dependency(ReplaceServices = true)]
public class MainDemoBlazorHostClientBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Quartz Admin";
    public override string LogoUrl => "logo.svg";
}
