using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.Blazor.Services;
using System;

namespace Syrna.QuartzAdmin.Blazor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartzAdminUI(this IServiceCollection services,
            IConfiguration blazoriseUIConfiguration)
        {
            services.Configure<QuartzAdminUIOptions>(blazoriseUIConfiguration);

            var uiOptions = blazoriseUIConfiguration.Get<QuartzAdminUIOptions>();
            services.AddQuartzAdmin(blazoriseUIConfiguration);

            return AddQuartzAdminUI(services);
        }

        public static IServiceCollection AddQuartzAdminUI(this IServiceCollection services,
            Action<QuartzAdminUIOptions> configure = null)
        {
            if (configure == null)
            {
                services.AddOptions<QuartzAdminUIOptions>()
                    .Configure(opt =>
                    {
                    });
            }
            else
            {
                QuartzAdminUIOptions uiOptions = new();
                services.Configure(configure);
                services.AddQuartzAdmin(
                    o =>
                    {
                        o.AllowedJobAssemblyFiles = uiOptions.AllowedJobAssemblyFiles;
                        o.AutoMigrateDb = uiOptions.AutoMigrateDb;
                        o.DataStoreProvider = uiOptions.DataStoreProvider;
                        o.DisallowedJobTypes = uiOptions.DisallowedJobTypes;
                    });
            }

            return AddQuartzAdminUI(services);
        }

        private static IServiceCollection AddQuartzAdminUI(IServiceCollection services)
        {
            services.AddTransient<ITriggerDetailModelValidator, TriggerDetailModelValidator>();
            services.AddSingleton<IJobUIProvider, JobUIProvider>();

            return services;
        }

    }
}

