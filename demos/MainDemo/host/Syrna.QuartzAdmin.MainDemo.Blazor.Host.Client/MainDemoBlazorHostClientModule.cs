using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using IdentityModel;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.Blazor.Services;
using Syrna.QuartzAdmin.MainDemo.Blazor.Host.Client.Menus;
using Syrna.QuartzAdmin.MainDemo.Blazor.WebAssembly;
using Syrna.QuartzAdmin.MainDemo.Jobs;
using System;
using System.Net.Http;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Components.WebAssembly.LeptonXLiteTheme;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Blazor.WebAssembly;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Blazor.WebAssembly;
using Volo.Abp.TenantManagement.Blazor.WebAssembly;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Host.Client;

[DependsOn(typeof(AbpAutofacWebAssemblyModule))]
//[DependsOn(typeof(AbpAspNetCoreComponentsWebAssemblyBasicThemeModule))]
[DependsOn(typeof(AbpAspNetCoreComponentsWebAssemblyLeptonXLiteThemeModule))]
[DependsOn(typeof(AbpAccountApplicationContractsModule))]
[DependsOn(typeof(AbpIdentityBlazorWebAssemblyModule))]
[DependsOn(typeof(AbpTenantManagementBlazorWebAssemblyModule))]
[DependsOn(typeof(AbpSettingManagementBlazorWebAssemblyModule))]
//
[DependsOn(typeof(MainDemoBlazorWebAssemblyModule))]
[DependsOn(typeof(MainDemoJobsModule))]
public class MainDemoBlazorHostClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        ConfigureAuthentication(builder);
        ConfigureHttpClient(context, environment);
        ConfigureBlazorise(context);
        ConfigureRouter(context);
        ConfigureMenu(context);
        ConfigureAutoMapper(context);
        ConfigureQuartzAdmin(context);
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(MainDemoBlazorHostClientModule).Assembly;
        });
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new MainDemoBlazorHostClientMenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        context.Services
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
    }

    private void ConfigureQuartzAdmin(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ITriggerDetailModelValidator, TriggerDetailModelValidator>();
        context.Services.AddSingleton<IJobUIProvider, JobUIProvider>();
    }

    private static void ConfigureAuthentication(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("AuthServer", options.ProviderOptions);
            options.UserOptions.RoleClaim = JwtClaimTypes.Role;
            //options.ProviderOptions.DefaultScopes.Add(OpenIddictConstants.Scopes.OfflineAccess);
            options.ProviderOptions.DefaultScopes.Add(OpenIddictConstants.Scopes.OpenId);
            options.ProviderOptions.DefaultScopes.Add(OpenIddictConstants.Scopes.Profile);
            options.ProviderOptions.DefaultScopes.Add(OpenIddictConstants.Scopes.Roles);
            options.ProviderOptions.DefaultScopes.Add(OpenIddictConstants.Scopes.Email);
            options.ProviderOptions.DefaultScopes.Add(OpenIddictConstants.Scopes.Phone);
            options.ProviderOptions.DefaultScopes.Add("QuartzAdmin");
        });
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }

    private void ConfigureAutoMapper(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MainDemoBlazorHostClientModule>();
        });
    }
}
