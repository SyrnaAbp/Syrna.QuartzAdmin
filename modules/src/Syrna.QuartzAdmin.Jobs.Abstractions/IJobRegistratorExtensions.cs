using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public static class IJobRegistratorExtensions
    {
        public static IJobRegistrator RegiserCRONJob<TJob>(
            this IJobRegistrator jobRegistrator,
            Action<JobOptions> jobOptions)
            where TJob : class, IJob
        {
            var options = new JobOptions();
            jobOptions?.Invoke(options);

            return jobRegistrator.RegiserJob<TJob>(
                options.Triggers?.Select(n => n.CreateTriggerBuilder())
                );
        }

        public static IJobRegistrator RegiserJob<TJob>(
            this IJobRegistrator jobRegistrator,
            Func<IEnumerable<TriggerBuilder>> triggers)
            where TJob : class, IJob
        {
            return jobRegistrator.RegiserJob<TJob>(triggers());
        }

        public static IServiceCollection AddQuartzJobDetail(this IServiceCollection services, Func<IJobDetail> detail)
        {
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(detail(), new List<ITrigger>()));
            return services;
        }

        public static IServiceCollection AddQuartzJob<TJob>(this IServiceCollection services, string identity) where TJob : class
        {
            return services.AddQuartzJob<TJob>(identity, null);
        }

        public static IServiceCollection AddQuartzJob<TJob>(this IServiceCollection services, string identity, string description) where TJob : class
        {
            return services.AddQuartzJob(typeof(TJob), identity, description);
        }

        public static IServiceCollection AddQuartzJob(this IServiceCollection services, Type t, string identity, string description)
        {
            if (!services.Any(sd => sd.ServiceType == t))
            {
                services.AddTransient(t);
            }
            var jobDetail = JobBuilder.Create(t).WithIdentity(identity).WithDescription(description).Build();
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(jobDetail, new List<ITrigger>()));
            return services;
        }

        public static IServiceCollection AddQuartzJobDetail(this IServiceCollection services, IJobDetail detail)
        {
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(detail, new List<ITrigger>()));
            return services;
        }

        public static IServiceCollection AddQuartzJob<TJob>(this IServiceCollection services) where TJob : class
        {
            services.AddTransient<TJob>();
            var jobDetail = JobBuilder.Create(typeof(TJob)).Build();
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(jobDetail, new List<ITrigger>()));
            return services;
        }

        public static IJobRegistrator RegiserJob<TJob>(
            this IJobRegistrator jobRegistrator,
            IEnumerable<TriggerBuilder> triggerBuilders)
            where TJob : class, IJob
        {
            jobRegistrator.Services.AddTransient<TJob>();

            var jobDetail = JobBuilder.Create<TJob>().Build();

            var triggers = new List<ITrigger>(triggerBuilders.Count());
            foreach (var triggerBuilder in triggerBuilders)
            {
                triggers.Add(triggerBuilder.ForJob(jobDetail).Build());
            }
            jobRegistrator.Services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(jobDetail, triggers));

            return jobRegistrator;
        }
    }
}