using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Server.Host
{
    [Dependency(ReplaceServices = true)]
    public class MainDemoBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "QuartzAdmin";
    }
}
