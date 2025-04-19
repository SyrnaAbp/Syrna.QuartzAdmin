using Quartz.Spi;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using Syrna.QuartzAdmin.Scheduler;
using System.Reflection;

namespace Syrna.QuartzAdmin.MainDemo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuartzAdminMain(
        this IServiceCollection services,
        IConfiguration quartzAdminUIConfiguration,
        Func<List<Assembly>>? jobsasmlist = null)
    {
        services.Configure<QuartzAdminUIOptions>(quartzAdminUIConfiguration);

        var uiOptions = quartzAdminUIConfiguration.Get<QuartzAdminUIOptions>();

        services.AddQuartzAdminJobs();
        services.AddSingleton<ISchedulerDefinitionService, SchedulerDefinitionService>();
        services.AddSingleton<IJobFactory, ServiceCollectionJobFactory>();

        var bb = new JobRegistrator(services);
        var types = JobsListHelper.GetQuartzAdminJobs(jobsasmlist?.Invoke());
        types.ForEach(t =>
        {
            var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
            if (so == null)
            {
                services.AddQuartzJob(t, t.Name, t.FullName);
            }
            else
            {
                services.AddQuartzJob(t, so.Identity ?? t.Name, so.Description ?? t.FullName);
            }
        });
        services.AddQuartzAdmin(quartzAdminUIConfiguration);

        return services;
    }

    public static IServiceCollection AddQuartzAdmin(this IServiceCollection services, IConfiguration? config = null)
    {
        QuartzAdminCoreOptions? coreOptions = null;
        if (config != null)
        {
            services.Configure<QuartzAdminCoreOptions>(config);
            coreOptions = config.Get<QuartzAdminCoreOptions>();
        }
        else
        {
            services.AddOptions<QuartzAdminCoreOptions>()
                .Configure(opt =>
                {
                });
        }
        return services;
    }
}
