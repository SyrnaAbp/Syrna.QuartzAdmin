using System;
using System.IO;
using System.Net.Http;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Components.Server.BasicTheme;
using Volo.Abp.AspNetCore.Components.Server.BasicTheme.Bundling;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Identity.Blazor.Server;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement.Blazor.Server;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Blazor.Server;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Account.Web;
using Volo.Abp.OpenIddict;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Syrna.QuartzAdmin.MainDemo.Blazor.Server.Host.Menus;
using Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore;
using Syrna.QuartzAdmin.MainDemo.Localization;
using Syrna.QuartzAdmin.MainDemo.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Server.Host
{
    //[DependsOn(typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule))]
    [DependsOn(typeof(AbpAspNetCoreMvcUiBasicThemeModule))]
    [DependsOn(typeof(AbpAspNetCoreComponentsServerBasicThemeModule))]

    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpSwashbuckleModule))]
    [DependsOn(typeof(AbpAspNetCoreAuthenticationJwtBearerModule))]
    [DependsOn(typeof(AbpAspNetCoreSerilogModule))]
    [DependsOn(typeof(AbpIdentityBlazorServerModule))]
    [DependsOn(typeof(AbpTenantManagementBlazorServerModule))]
    [DependsOn(typeof(AbpSettingManagementBlazorServerModule))]

    [DependsOn(typeof(MainDemoHttpApiModule))]
    [DependsOn(typeof(MainDemoEntityFrameworkCoreModule))]
    [DependsOn(typeof(MainDemoApplicationModule))]

    [DependsOn(typeof(AbpAccountWebModule))]
    [DependsOn(typeof(AbpAccountWebOpenIddictModule))]

    [DependsOn(typeof(MainDemoBlazorServerModule))]
    public class MainDemoBlazorHostModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            PreConfigure<OpenIddictBuilder>(builder =>
            {
                builder.AddValidation(options =>
                {
                    //options.SetIssuer("https://syrnaids.syrna.net/");
                    options.SetIssuer(configuration["AuthServer:Authority"]);
                    options.AddAudiences("QuartzAdmin");
                    //options.UseLocalServer();
                    options.UseAspNetCore();
                    options.UseSystemNetHttp();
                });
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
                    //x.AddProductionEncryptionAndSigningCertificate($"{pfxFile}", "ddd364f4-15e3-494a-bdbc-2fd930db96bb");
                    x.AddSigningCertificate(GetSigningCertificate(hostingEnvironment, configuration));
                    x.AddEncryptionCertificate(GetSigningCertificate(hostingEnvironment, configuration));

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
                    x.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow();
                });
            }

            context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
            {
                options.AddAssemblyResource(
                    typeof(MainDemoResource),
                    typeof(MainDemoDomainModule).Assembly,
                    typeof(MainDemoDomainSharedModule).Assembly,
                    typeof(MainDemoApplicationModule).Assembly,
                    typeof(MainDemoApplicationContractsModule).Assembly,
                    typeof(MainDemoBlazorHostModule).Assembly
                );
            });
        }

        private static X509Certificate2 GetSigningCertificate(IWebHostEnvironment hostingEnv, IConfiguration configuration)
        {
            var fileName = "openiddict.pfx";
            var passPhrase = "ddd364f4-15e3-494a-bdbc-2fd930db96bb";
            var file = Path.Combine(hostingEnv.ContentRootPath, fileName);

            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"Signing Certificate couldn't found: {file}");
            }

            try
            {
                var c = new X509Certificate2(file, passPhrase, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
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

            Configure<AbpBundlingOptions>(options =>
            {
                // MVC UI
                options.StyleBundles.Configure(
                    BasicThemeBundles.Styles.Global,
                    bundle =>
                    {
                        bundle.AddFiles("/global-styles.css");
                    }
                );

                //BLAZOR UI
                options.StyleBundles.Configure(
                    BlazorBasicThemeBundles.Styles.Global,
                    bundle =>
                    {
                        bundle.AddFiles("/blazor-global-styles.css");
                        //You can remove the following line if you don't use Blazor CSS isolation for components
                        bundle.AddFiles("/Syrna.QuartzAdmin.MainDemo.Blazor.Server.Host.styles.css");
                    }
                );
            });

            context.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AuthServer:Authority"];
                    options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                    options.Audience = "QuartzAdmin";
                });

            if (hostingEnvironment.IsDevelopment())
            {
                Configure<AbpVirtualFileSystemOptions>(options =>
                {
                    options.FileSets.ReplaceEmbeddedByPhysical<MainDemoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Domain.Shared", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<MainDemoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Domain", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<MainDemoApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Application.Contracts", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<MainDemoApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Syrna.QuartzAdmin.MainDemo.Application", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<MainDemoBlazorHostModule>(hostingEnvironment.ContentRootPath);
                });
            }

            context.Services.AddAbpSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "QuartzAdmin API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                });

            Configure<AbpMultiTenancyOptions>(options =>
            {
                options.IsEnabled = MultiTenancyConsts.IsEnabled;
            });

            context.Services.AddTransient(sp => new HttpClient
            {
                BaseAddress = new Uri("/")
            });

            context.Services
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons();

            Configure<AbpNavigationOptions>(options =>
            {
                options.MenuContributors.Add(new MainDemoMenuContributor());
            });

            Configure<AbpRouterOptions>(options =>
            {
                options.AppAssembly = typeof(MainDemoBlazorHostModule).Assembly;
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var env = context.GetEnvironment();
            var app = context.GetApplicationBuilder();

            app.UseAbpRequestLocalization();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCorrelationId();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            if (MultiTenancyConsts.IsEnabled)
            {
                app.UseMultiTenancy();
            }

            app.UseUnitOfWork();
            app.UseAbpOpenIddictValidation();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuartzAdmin API");
            });
            app.UseConfiguredEndpoints();
        }
    }
}
