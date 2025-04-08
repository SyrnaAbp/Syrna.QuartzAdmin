using Syrna.QuartzAdmin.MainDemo.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Server.Host
{
    public abstract class MainDemoComponentBase : AbpComponentBase
    {
        protected MainDemoComponentBase()
        {
            LocalizationResource = typeof(MainDemoResource);
        }
    }
}
