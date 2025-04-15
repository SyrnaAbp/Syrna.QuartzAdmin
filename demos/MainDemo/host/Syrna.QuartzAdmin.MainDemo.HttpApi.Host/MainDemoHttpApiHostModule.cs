using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Util;
using Serilog;
using StackExchange.Redis;
using Syrna.QuartzAdmin.ExecutionHistory;
using Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore;
using Syrna.QuartzAdmin.MainDemo.Jobs;
using Syrna.QuartzAdmin.MainDemo.MultiTenancy;
using System.Security.Cryptography.X509Certificates;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.FluentValidation;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Quartz;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Syrna.QuartzAdmin.MainDemo;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpCachingStackExchangeRedisModule))]
[DependsOn(typeof(AbpAspNetCoreMvcUiMultiTenancyModule))]
[DependsOn(typeof(AbpIdentityAspNetCoreModule))]
[DependsOn(typeof(AbpSwashbuckleModule))]
[DependsOn(typeof(AbpAccountWebOpenIddictModule))]
[DependsOn(typeof(AbpAspNetCoreSerilogModule))]
[DependsOn(typeof(AbpAspNetCoreMvcUiBasicThemeModule))]
//[DependsOn(typeof(HasAspNetCoreSerilogModule))]
[DependsOn(typeof(AbpFluentValidationModule))]
[DependsOn(typeof(AbpBackgroundWorkersModule))]
//[DependsOn(typeof(HasAspNetCoreHttpOverridesModule))]

[DependsOn(typeof(MainDemoHttpApiModule))]
[DependsOn(typeof(MainDemoApplicationModule))]
[DependsOn(typeof(MainDemoEntityFrameworkCoreModule))]
//[DependsOn(typeof(MainDemoEntityFrameworkCorePostgreSqlModule))]
[DependsOn(typeof(AbpQuartzModule))]
[DependsOn(typeof(MainDemoJobsModule))]

public class MainDemoHttpApiHostModule : AbpModule
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
                options.SetIssuer(configuration["AuthServer:Authority"]!);
                options.AddAudiences("QuartzAdmin", "QuartzAdmin API");
                //options.UseLocalServer();
                options.UseAspNetCore();
                options.UseSystemNetHttp();
            });
        });

        PreConfigure<OpenIddictServerBuilder>(x =>
        {
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
            x.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow().AllowPasswordFlow();
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
                //x.AddProductionEncryptionAndSigningCertificate($"{pfxFile}", "266657b3-2d03-4888-b9ee-b3f0939e9e24");
                x.AddSigningCertificate(GetSigningCertificate(hostingEnvironment));
                x.AddEncryptionCertificate(GetSigningCertificate(hostingEnvironment));
                //x.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", "266657b3-2d03-4888-b9ee-b3f0939e9e24");
            });
        }

        var quartzEnable = configuration["Quartz:Enabled"];
        if (quartzEnable?.ToLower() == "true")
        {
            PreConfigure<AbpQuartzOptions>(options =>
            {
                options.Configurator = configure =>
                {
                    configure.SchedulerId = configuration["Quartz:SchedulerId"] ?? "QNOC";
                    configure.SchedulerName = configuration["Quartz:SchedulerName"] ?? $"{Environment.MachineName}_Quartz";
                    configure.SetProperty("quartz.plugin.recentHistory.type", typeof(AbpExecutionHistoryPlugin).AssemblyQualifiedNameWithoutVersion());
                    configure.SetProperty("quartz.plugin.recentHistory.storeType", typeof(AbpExecutionHistoryStore).AssemblyQualifiedNameWithoutVersion());
                    configure.UsePersistentStore(storeOptions =>
                    {
                        storeOptions.UseProperties = true;
                        storeOptions.PerformSchemaValidation = false;
                        storeOptions.UseNewtonsoftJsonSerializer();
                        storeOptions.UsePostgres(configurer =>
                        {
                            configurer.UseDriverDelegate<PostgreSQLDelegate>();
                            configurer.TablePrefix = "quartz.qrtz_";
                            configurer.ConnectionStringName = "Default";
                        });
                        storeOptions.UseClustering(c =>
                        {
                            c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                            c.CheckinInterval = TimeSpan.FromSeconds(10);
                        });
                    });
                    //configure.AddSchedulerListener<SampleSchedulerListener>();
                };
            });
            context.Services.AddQuartzAdminMain(configuration.GetSection("QuartzAdmin"));
        }
    }

    private static void AutoLocalizationResourceContributors(IServiceCollection services)
    {
        //var contributors = new List<Type>();

        //services.OnRegistered(context =>
        //{
        //    if (typeof(ILocalizationResourceContributor).IsAssignableFrom(context.ImplementationType))
        //    {
        //        contributors.Add(context.ImplementationType);
        //    }
        //});

        services.Configure<AbpLocalizationOptions>(options =>
        {
            //options.GlobalContributors.Clear();
            //foreach (var contributor in contributors)
            //{
            //    if (contributor.FullName != "Volo.Abp.Identity.IdentityPermissionDefinitionProvider")
            //    {
            //        options.GlobalContributors.AddIfNotContains(contributor);
            //    }
            //}

            //options.Resources.Clear();
            //options.Resources.Remove("");
        });
    }

    private static X509Certificate2 GetSigningCertificate(IWebHostEnvironment hostingEnv)
    {
        const string fileName = "openiddict.pfx";
        const string passPhrase = "ceae6457-5634-4e9f-8ea2-0be3ad54001a";
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
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        //AutoLocalizationResourceContributors(context.Services);

        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.All;
            options.RequireHeaderSymmetry = false;
            options.ForwardLimit = 3;
            options.KnownProxies.Clear();
            options.KnownNetworks.Clear();
        });

        ConfigureAuthentication(context);
        ConfigureAuthentication(context, configuration);
        //ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers();
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureJson(context.Services);
        ConfigureSwaggerServices(context, configuration);
        ConfigureBackgrounds(context);
        ConfigureExternalProviders(context);
        ConfigureCache(configuration);
        ConfigureRedis(context, configuration, hostingEnvironment);

        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.EntityHistorySelectors.AddAllEntities();
            options.EntityHistorySelectors.Add(
                new NamedTypeSelector(
                    "Abp.FullAuditedEntity",
                    type => typeof(IFullAuditedObject).IsAssignableFrom(type)
                )
            );
        });

    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        //context.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private static void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                //options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //NameClaimType = "sub",
                    //RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    //ValidateIssuerSigningKey = true,
                    ValidAudience = "QuartzAdmin",
                    ValidAudiences = new[] { "QuartzAdmin", "QuartzAdmin API" },
                    ValidIssuer = configuration["AuthServer:Authority"]
                    //ValidTypes = ["at+jwt"],
                    //ValidAlgorithms = ["RS256"]
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthServer:SwaggerClientSecret"]))
                };
                options.UseSecurityTokenValidators = true;
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "QuartzAdmin";
                options.MapInboundClaims = false;
#if DEBUG
                options.IncludeErrorDetails = true;
#endif
            });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle => { bundle.AddFiles("/global-styles.css"); }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? []);

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

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
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(MainDemoApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(QuartzAdminApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                    {"QuartzAdmin", "QuartzAdmin API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "QuartzAdmin API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);

                //var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "Syrna.*.xml");
                foreach (var xmlFile in xmlFiles)
                {
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
                }
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? [])
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
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

        app.UseCorrelationId();
        app.UseHttpsRedirection();
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

        app.UseSwagger();
        app.UseAbpSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuartzAdmin API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            c.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
            c.OAuthScopes("QuartzAdmin");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        //TODO
        //app.UseMainDemoSerilogEnrichers();
        //app.UseQuartzAdmin();
        app.UseConfiguredEndpoints();
    }

    private void ConfigureJson(IServiceCollection services)
    {
        //services.AddControllers().AddNewtonsoftJson();
        //services.AddControllersWithViews().AddJsonOptions(options =>
        //    options.JsonSerializerOptions.PropertyNamingPolicy = null
        //);
        // serialization settings must be similar to the blazor project so both ends can understand each other
        // see more at https://docs.telerik.com/aspnet-core/compatibility/json-serialization#json-serialization
        // on sample JSON serialization settings for ASP.NET Core that aim at case-insensitive serialization
    }

    private void ConfigureBackgrounds(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundWorkerOptions>(options => { options.IsEnabled = false; });
    }

    private static void ConfigureExternalProviders(ServiceConfigurationContext context)
    {
        context
            .Services
            .AddAuthentication()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = ".......";
                options.ClientSecret = ".......";
            });

        context
            .Services
            .AddAuthentication()
            .AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = ".......";
                options.ClientSecret = ".......";
            });
        context
            .Services
            .AddAuthentication()
            .AddTwitter(TwitterDefaults.AuthenticationScheme, options =>
            {
                options.ConsumerKey = ".......";
                options.ConsumerSecret = ".......";
            });
    }

    private void ConfigureCache(IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "QuartzAdmin:"; });
    }

    private void ConfigureRedis(ServiceConfigurationContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            context.Services
                .AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, "QuartzAdmin-Protection-Keys");
        }
    }
}