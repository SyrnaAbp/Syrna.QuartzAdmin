using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.Localization;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Syrna.QuartzAdmin.Web
{
    [DependsOn(
        typeof(QuartzAdminApplicationContractsModule),
        typeof(AbpAspNetCoreMvcUiThemeSharedModule),
        typeof(AbpAutoMapperModule)
        )]
    public class QuartzAdminWebModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
            {
                options.AddAssemblyResource(typeof(QuartzAdminResource), typeof(QuartzAdminWebModule).Assembly);
            });

            PreConfigure<IMvcBuilder>(mvcBuilder =>
            {
                mvcBuilder.AddApplicationPartIfNotExists(typeof(QuartzAdminWebModule).Assembly);
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpToolbarOptions>(options =>
            {
                options.Contributors.Add(new QuartzAdminToolbarContributor());
            });

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<QuartzAdminWebModule>();
            });

            context.Services.AddAutoMapperObjectMapper<QuartzAdminWebModule>();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<QuartzAdminWebModule>(validate: true);
            });

            Configure<RazorPagesOptions>(options =>
            {
                //Configure authorization.
            });
            
            //Configure<AbpBundlingOptions>(options =>
            //{
            //    options
            //        .StyleBundles
            //        .Configure(StandardBundles.Styles.Global, bundle => {
            //            bundle.AddContributors(typeof(PmNotificationStyleBundleContributor));
            //        });
                
            //    options
            //        .ScriptBundles
            //        .Configure(StandardBundles.Scripts.Global, bundle => {
            //            bundle.AddContributors(typeof(PmNotificationScriptBundleContributor));
            //        });
            //});
        }
    }
}
