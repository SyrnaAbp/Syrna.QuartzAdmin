using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using IdentityModel;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Syrna.QuartzAdmin.MainDemo.Blazor.WebAssembly.Host.Menus;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Volo.Abp.AspNetCore.Components.Web;
using Volo.Abp.AspNetCore.Components.Web.BasicTheme.Themes.Basic;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Components.WebAssembly.BasicTheme;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.AutoMapper;
using Volo.Abp.BlazoriseUI;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Identity.Blazor.WebAssembly;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Blazor.WebAssembly;
using Volo.Abp.TenantManagement.Blazor.WebAssembly;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.WebAssembly.Host;

[DependsOn(typeof(AbpAutofacWebAssemblyModule))]
[DependsOn(typeof(AbpExceptionHandlingModule))]
[DependsOn(typeof(AbpAutoMapperModule))]
[DependsOn(typeof(MainDemoHttpApiClientModule))]

[DependsOn(typeof(AbpBlazoriseUIModule))]

[DependsOn(typeof(AbpAspNetCoreComponentsWebAssemblyBasicThemeModule))]
[DependsOn(typeof(AbpIdentityBlazorWebAssemblyModule))]
[DependsOn(typeof(AbpTenantManagementBlazorWebAssemblyModule))]
[DependsOn(typeof(AbpSettingManagementBlazorWebAssemblyModule))]
//app modules
[DependsOn(typeof(MainDemoBlazorWebAssemblyModule))]

public class MainDemoBlazorWebAssemblyHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAspNetCoreComponentsWebOptions>(options =>
        {
            options.IsBlazorWebApp = false;
        });
        AutoAddDefinitionProviders(context.Services);
        //AutoLocalizationResourceContributors(context.Services);
    }

    private static void AutoAddDefinitionProviders(IServiceCollection services)
    {
        var definitionProviders = new List<Type>();

        services.OnRegistered(context =>
        {
            if (typeof(IPermissionDefinitionProvider).IsAssignableFrom(context.ImplementationType))
            {
                definitionProviders.Add(context.ImplementationType);
            }
        });

        services.Configure<AbpPermissionOptions>(options =>
        {
            options.DefinitionProviders.Clear();
            foreach (var definitionProvider in definitionProviders)
            {
                if (definitionProvider.FullName != "Volo.Abp.Identity.IdentityPermissionDefinitionProvider")
                {
                    options.DefinitionProviders.AddIfNotContains(definitionProvider);
                }
            }
        });
    }

    private static void AutoLocalizationResourceContributors(IServiceCollection services)
    {
        var contributors = new List<Type>();

        services.OnRegistered(context =>
        {
            if (typeof(ILocalizationResourceContributor).IsAssignableFrom(context.ImplementationType))
            {
                contributors.Add(context.ImplementationType);
            }
        });

        services.Configure<AbpLocalizationOptions>(options =>
        {
            options.GlobalContributors.Clear();
            foreach (var contributor in contributors)
            {
                if (contributor.FullName != "Volo.Abp.Identity.IdentityPermissionDefinitionProvider")
                {
                    options.GlobalContributors.AddIfNotContains(contributor);
                }
            }
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        ConfigureAuthentication(builder);
        ConfigureHttpClient(context, environment);
        ConfigureBlazorise(context);
        ConfigureRouter(context);
        //ConfigureUi(builder);
        ConfigureMenu(context);
        ConfigureAutoMapper(context);

        context.Services.AddAutoMapperObjectMapper<MainDemoBlazorWebAssemblyHostModule>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddProfile<MainDemoBlazorWebAssemblyHostAutoMapperProfile>(validate: true);
        });

        //Configure<SettingManagementComponentOptions>(options =>
        //{
        //    //options.Contributors.Add(new AlphaHostSettingComponentContributor());
        //    //options.Contributors.Add(new TimeZonePageContributor());
        //});
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(MainDemoBlazorWebAssemblyHostModule).Assembly;
        });
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new MainDemoBlazorWebAssemblyHostMenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        context.Services
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
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

    //private static void ConfigureUi(WebAssemblyHostBuilder builder)
    //{
    //    builder.RootComponents.Add<App>("#ApplicationContainer");
    //}

    //private static void ConfigureTelerikBlazor(WebAssemblyHostBuilder builder)
    //{
    //    builder.Services.AddTelerikBlazor();

    //    // register the LocalStorage service
    //    builder.Services.AddScoped<LocalStorage>();
    //}

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
            options.AddMaps<MainDemoBlazorWebAssemblyHostModule>();
        });
    }
}