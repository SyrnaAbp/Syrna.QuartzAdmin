using Localization.Resources.AbpUi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Serilog;
using StackExchange.Redis;
using Syrna.Alpha.SilkierQuartz.PostgreSql.EntityFrameworkCore;
using Syrna.QuartzAdmin.MainDemo.Localization;
using Syrna.QuartzAdmin.MainDemo.MultiTenancy;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Localization;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Syrna.QuartzAdmin.MainDemo;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpCachingStackExchangeRedisModule))]
[DependsOn(typeof(AbpAspNetCoreSerilogModule))]
[DependsOn(typeof(AbpAccountWebOpenIddictModule))]
[DependsOn(typeof(AbpAspNetCoreMvcUiBasicThemeModule))]
[DependsOn(typeof(AbpAccountApplicationModule))]
[DependsOn(typeof(AbpSettingsModule))]
[DependsOn(typeof(AbpAccountWebModule))]
//[DependsOn(typeof(AbpAspNetCoreComponentsWebBasicThemeModule))]
//app
[DependsOn(typeof(MainDemoEntityFrameworkCorePostgreSqlModule))]
[DependsOn(typeof(MainDemoApplicationModule))]
public class MainDemoAuthServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                //options.SetIssuer("https://csbsids.saglik.gov.tr/");
                options.SetIssuer(configuration["App:SelfUrl"]);
                options.AddAudiences("QuartzAdmin", "QuartzAdmin API");
                //options.UseLocalServer();
                options.UseAspNetCore();
                options.UseSystemNetHttp();
            });
        });

        PreConfigure<OpenIddictServerBuilder>(x =>
        {
            // Enable the authorization, logout, token and userinfo endpoints.
            x.SetAuthorizationEndpointUris("connect/authorize", "connect/authorize/callback")
                .SetEndSessionEndpointUris("connect/endsession")
                .SetTokenEndpointUris("connect/token")
                .SetUserInfoEndpointUris("connect/userinfo")
                .SetIntrospectionEndpointUris("connect/introspect")
                .SetDeviceAuthorizationEndpointUris("device")
                .SetEndUserVerificationEndpointUris("connect/verify")
                .SetRevocationEndpointUris("connect/revocat");

            //scope: 'offline_access openid profile role email phone QuartzAdmin',
            x.RegisterScopes(
                OpenIddictConstants.Scopes.OfflineAccess,
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Phone,
                "QuartzAdmin"
            );
            x.AllowAuthorizationCodeFlow();
            x.AllowDeviceAuthorizationFlow();
            x.AllowRefreshTokenFlow();
            x.AllowClientCredentialsFlow();
            x.AllowPasswordFlow();

            // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
            x.UseAspNetCore()
                .EnableAuthorizationEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .EnableUserInfoEndpointPassthrough()
                .EnableEndSessionEndpointPassthrough()
                .EnableEndUserVerificationEndpointPassthrough()
                .EnableStatusCodePagesIntegration();
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(x =>
            {
                //var pfxFile = Path.Combine(hostingEnvironment.ContentRootPath, "openiddict.pfx");
                //x.AddProductionEncryptionAndSigningCertificate($"{pfxFile}", "82fca703-ad8c-41de-a023-6316fe2e2a89");
                x.AddSigningCertificate(GetSigningCertificate(hostingEnvironment, configuration));
                x.AddEncryptionCertificate(GetSigningCertificate(hostingEnvironment, configuration));
            });
        }
    }

    private static X509Certificate2 GetSigningCertificate(IWebHostEnvironment hostingEnv, IConfiguration configuration)
    {
        var fileName = "openiddict.pfx";
        var passPhrase = "82fca703-ad8c-41de-a023-6316fe2e2a89";
        var file = Path.Combine(hostingEnv.ContentRootPath, fileName);

        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"Signing Certificate couldn't found: {file}");
        }

        try
        {
            var c = new X509Certificate2(file, passPhrase);
            return c;
        }
        catch (Exception e)
        {
            Log.Fatal(e.InnerException ?? e, $"Signing Certificate couldn't load: {file}");
            throw;
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.All;
            options.RequireHeaderSymmetry = false;
            options.ForwardLimit = 3;
            options.KnownProxies.Clear();
            options.KnownNetworks.Clear();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<MainDemoResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource),
                    typeof(AccountResource)
                );
        });

        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle => { bundle.AddFiles("/global-styles.css"); }
            );
        });

        Configure<AbpAuditingOptions>(options =>
        {
            //options.IsEnabledForGetRequests = true;
            options.ApplicationName = "AuthServer";
        });

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<MainDemoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<MainDemoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<MainDemoApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<MainDemoApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Application", Path.DirectorySeparatorChar)));
            });
        }

        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? []);

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });

        ConfigureBackgroundJobs(configuration);
        ConfigureBackgroundWorkers(configuration);

        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "QuartzAdmin:"; });

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("QuartzAdmin");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "QuartzAdmin-Protection-Keys");
        }

        //context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        //{
        //    var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
        //    return new RedisDistributedSynchronizationProvider(redis.GetDatabase());
        //});

        ConfigureCors(context, configuration);

        //context.Services.AddSameSiteCookiePolicy();

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private static void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? []
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureBackgroundJobs(IConfiguration configuration)
    {
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = Convert.ToBoolean(configuration["BackgroundJobs:Enabled"]);
        });
    }

    private void ConfigureBackgroundWorkers(IConfiguration configuration)
    {
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = Convert.ToBoolean(configuration["BackgroundWorkers:Enabled"]);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        //app.UseMiddleware<RequestResponseLoggingMiddleware>();
        //app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);

        app.UseCorrelationId();
        //app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}