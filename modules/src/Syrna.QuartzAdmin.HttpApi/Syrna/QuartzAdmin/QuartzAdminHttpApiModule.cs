using Localization.Resources.AbpUi;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.Localization;
using Volo.Abp.AspNetCore.Mvc.Localization;

namespace Syrna.QuartzAdmin
{
    [DependsOn(
        typeof(QuartzAdminApplicationContractsModule),
        typeof(AbpAspNetCoreMvcModule))]
    public class QuartzAdminHttpApiModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
            {
                options.AddAssemblyResource(
                    typeof(QuartzAdminResource),
                    typeof(QuartzAdminApplicationContractsModule).Assembly);
            });

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
