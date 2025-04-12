using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Reflection;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///  Returns a client-usable handle to a Quartz.IScheduler.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IScheduler GetScheduler(this IApplicationBuilder app)
        {
            return app.GetRequiredService<ISchedulerFactory>().GetScheduler().Result;
        }
        /// <summary>
        ///  Returns a handle to the Scheduler with the given name, if it exists.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="schedName"></param>
        /// <returns></returns>
        public static IScheduler GetScheduler(this IServiceCollection app, string schedName)
        {
            return app.GetRequiredService<ISchedulerFactory>().GetScheduler(schedName).Result;
        }
        /// <summary>
        /// Returns handles to all known Schedulers (made by any SchedulerFactory within  this app domain.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IReadOnlyList<IScheduler> GetAllSchedulers(this IServiceProvider app)
        {
            return app.GetRequiredService<ISchedulerFactory>().GetAllSchedulers().Result;
        }

        /// <summary>
        /// Use QuartzAdmin and automatically discover IJob subclasses with QuartzTriggerAttribute
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configure"></param>
        public static ServiceConfigurationContext UseQuartzAdmin(this ServiceConfigurationContext context)
        {

            var types = JobsListHelper.GetQuartzAdminJobs();
            types.ForEach(t =>
            {
                var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
                context.UseQuartzJob(t, () =>
                {
                    if (!so.Manual)
                    {
                        var tb = TriggerBuilder.Create();
                        tb.WithSimpleSchedule(x =>
                        {
                            x.WithInterval(so.WithInterval);
                            if (so.RepeatCount > 0)
                            {
                                x.WithRepeatCount(so.RepeatCount);

                            }
                            else
                            {
                                x.RepeatForever();
                            }
                        });
                        if (so.StartAt == DateTimeOffset.MinValue)
                        {
                            tb.StartNow();
                        }
                        else
                        {
                            tb.StartAt(so.StartAt);
                        }

                        var tk = new TriggerKey(!string.IsNullOrEmpty(so.TriggerName) ? so.TriggerName : $"{t.Name}'s Trigger");
                        if (!string.IsNullOrEmpty(so.TriggerGroup))
                        {
                            so.TriggerGroup = so.TriggerGroup;
                        }
                        tb.WithIdentity(tk);
                        tb.WithDescription(so.TriggerDescription ?? $"{t.Name}'s Trigger,full name is {t.FullName}");
                        if (so.Priority > 0) tb.WithPriority(so.Priority);
                        return tb;
                    }
                    else
                    {
                        return null;
                    }
                });

            });
            return context;
        }

        public static ServiceConfigurationContext UseQuartzJob(this ServiceConfigurationContext app, Type t, Func<TriggerBuilder> triggerBuilders_func)
        {
            var lst = new List<TriggerBuilder>();
            var tb = triggerBuilders_func?.Invoke();
            if (tb != null)
            {
                lst.Add(tb);
            }
            return app.UseQuartzJob(t, lst);
        }

        public static ServiceConfigurationContext UseQuartzJob(this ServiceConfigurationContext app, Type t, Func<IEnumerable<TriggerBuilder>> triggerBuilders_func)
        {
            return app.UseQuartzJob(t, triggerBuilders_func());
        }

        public static ServiceConfigurationContext UseQuartzJob(this ServiceConfigurationContext app, Type t, IEnumerable<TriggerBuilder> triggerBuilders)
        {
            var _scheduleJobs = app.Services.GetRequiredService<IEnumerable<IScheduleJob>>();
            var job = from js in _scheduleJobs where js.JobDetail.JobType == t select js;
            if (job.Any())
            {
                var scheduleJob = job.First();
                var lstgs = (List<ITrigger>)scheduleJob.Triggers;
                triggerBuilders.ToList().ForEach(triggerBuilder =>
                {
                    lstgs.Add(triggerBuilder.ForJob(scheduleJob.JobDetail).Build());
                });

            }
            return app;
        }
    }
}

