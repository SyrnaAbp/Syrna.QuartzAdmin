using Localization.Resources.AbpUi;
using Syrna.QuartzAdmin.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminApplicationContractsModule),
        typeof(AbpAspNetCoreMvcModule))]
    public class QuartzAdminHttpApiModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<IMvcBuilder>(mvcBuilder =>
            {
                mvcBuilder.AddApplicationPartIfNotExists(typeof(QuartzAdminHttpApiModule).Assembly);
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Get<QuartzAdminResource>()
                    .AddBaseTypes(typeof(AbpUiResource));
            });
        }
    }
}
