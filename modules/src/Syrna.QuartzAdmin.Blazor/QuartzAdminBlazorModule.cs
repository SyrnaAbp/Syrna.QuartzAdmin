using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Components.Web.Theming;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Components.Web.Theming.Toolbars;
using Volo.Abp.AutoMapper;
using Volo.Abp.BlazoriseUI;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.Blazor
{
    [DependsOn(
        typeof(QuartzAdminApplicationContractsModule),
        typeof(AbpAspNetCoreComponentsWebThemingModule),
        typeof(AbpAutoMapperModule),
        typeof(AbpBlazoriseUIModule)
        )]
    public class QuartzAdminBlazorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpToolbarOptions>(options =>
            {
                options.Contributors.Add(new QuartzAdminToolbarContributor());
            });

            context.Services.AddAutoMapperObjectMapper<QuartzAdminBlazorModule>();

            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddProfile<QuartzAdminBlazorAutoMapperProfile>(validate: true);
            });

            context.Services.AddAutoMapperObjectMapper<QuartzAdminBlazorModule>();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<QuartzAdminBlazorModule>(validate: true);
            });
            
            //Configure<AbpNavigationOptions>(options =>
            //{
            //    options.MenuContributors.Add(new QuartzAdminMenuContributor());
            //});

            Configure<AbpRouterOptions>(options =>
            {
                options.AdditionalAssemblies.Add(typeof(QuartzAdminBlazorModule).Assembly);
            });
        }
    }
}