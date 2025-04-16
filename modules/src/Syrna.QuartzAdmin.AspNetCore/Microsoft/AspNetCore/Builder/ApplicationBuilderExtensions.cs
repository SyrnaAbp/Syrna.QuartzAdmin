using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz;
using Quartz.Impl;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///  Returns a client-usable handle to a Quartz.IScheduler.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IScheduler GetScheduler(this IApplicationBuilder app)
    {
        return app.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler().Result;
    }
    /// <summary>
    ///  Returns a handle to the Scheduler with the given name, if it exists.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="schedName"></param>
    /// <returns></returns>
    public static IScheduler GetScheduler(this IApplicationBuilder app, string schedName)
    {
        return app.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler(schedName).Result;
    }
    /// <summary>
    /// Returns handles to all known Schedulers (made by any SchedulerFactory within  this app domain.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IReadOnlyList<IScheduler> GetAllSchedulers(this IApplicationBuilder app)
    {
        return app.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetAllSchedulers().Result;
    }

    /// <summary>
    /// Use QuartzAdmin and automatically discover IJob subclasses with QuartzAdminAttribute
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configure"></param>
    public static IApplicationBuilder UseQuartzAdmin(this IApplicationBuilder app/*, Action<Services> configure = null*/)
    {
        //var options = app.ApplicationServices
        //    .GetService<QuartzAdminOptions>() ?? throw new ArgumentNullException(nameof(QuartzAdminOptions));
        //var authenticationOptions = app.ApplicationServices
        //    .GetService<QuartzAdminAuthenticationOptions>();

        //app.UseFileServer(options);
        //if (options.Scheduler == null)
        //{
        //    try
        //    {
        //        options.Scheduler = app.ApplicationServices.GetRequiredService<ISchedulerFactory>()?.GetScheduler().Result;
        //    }
        //    catch (Exception)
        //    {
        //        options.Scheduler = null;
        //    }
        //    if (options.Scheduler == null)
        //    {
        //        options.Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
        //    }
        //}
        //var services = Services.Create(options, authenticationOptions);
        //configure?.Invoke(services);

        //app.Use(async (context, next) =>
        //{
        //    context.Items[typeof(Services)] = services;
        //    await next.Invoke();
        //});

        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllerRoute(nameof(QuartzAdmin), $"{options.VirtualPathRoot}/{{controller=Scheduler}}/{{action=Index}}");
        //    endpoints.MapControllerRoute($"{nameof(QuartzAdmin)}Authenticate",
        //        $"{options.VirtualPathRoot}/{{controller=Authenticate}}/{{action=Login}}");
        //});

        var type = typeof(IJob);
        var types = AutoJobsListHelper.GetQuartzAdminJobs();
        types.ForEach(t =>
        {
            var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
            app.UseQuartzJob(t, () =>
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

        return app;
    }

    public static IServiceCollection UseQuartzAdmin(this IServiceCollection app/*, Action<Services> configure = null*/)
    {
        var type = typeof(IJob);
        var types = AutoJobsListHelper.GetQuartzAdminJobs();
        types.ForEach(t =>
        {
            var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
            app.UseQuartzJob(t, () =>
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

        return app;
    }

    //private static void UseFileServer(this IApplicationBuilder app, QuartzAdminOptions options)
    //{
    //    IFileProvider fs;

    //    var manifestEmbeddedProvider =
    //        new EmbeddedFileProvider(typeof(QuartzAdminOptions).Assembly);


    //    fs = new EmbeddedFileProvider(typeof(QuartzAdminOptions).Assembly, "QuartzAdmin.Content");
    //    var fsOptions = new FileServerOptions()
    //    {
    //        RequestPath = new PathString($"{options.VirtualPathRoot}/Content"),
    //        EnableDefaultFiles = false,
    //        EnableDirectoryBrowsing = false,
    //        FileProvider = fs
    //    };

    //    app.UseFileServer(fsOptions);
    //}

    public static IApplicationBuilder UseQuartzJob<TJob>(
            this IApplicationBuilder app,
            Func<TriggerBuilder> triggerBuilder_func)
            where TJob : class, IJob
    {
        return app.UseQuartzJob<TJob>(new TriggerBuilder[] { triggerBuilder_func() });
    }

    public static IApplicationBuilder UseQuartzJob<TJob>(
        this IApplicationBuilder app, string JobKey,
        Func<TriggerBuilder> triggerBuilder_func)
        where TJob : class, IJob
    {
        var _scheduleJobs = app.ApplicationServices.GetService<IEnumerable<IScheduleJob>>();

        var job = from js in _scheduleJobs where js.JobDetail.JobType == typeof(TJob) && js.JobDetail.Key.Name == JobKey select js;
        if (job.Any())
        {
            var scheduleJob = job.First();
            var lstgs = (List<ITrigger>)scheduleJob.Triggers;
            lstgs.Add(triggerBuilder_func().ForJob(scheduleJob.JobDetail).Build());
        }
        return app;
    }

    public static IApplicationBuilder UseQuartzJob<TJob>(
            this IApplicationBuilder app,
            TriggerBuilder triggerBuilder)
            where TJob : class, IJob
    {
        return app.UseQuartzJob<TJob>(new TriggerBuilder[] { triggerBuilder });
    }

    public static IApplicationBuilder UseQuartzJob<TJob>(
        this IApplicationBuilder app,
        Func<IEnumerable<TriggerBuilder>> triggerBuilders_func)
        where TJob : class, IJob
    {
        return app.UseQuartzJob<TJob>(triggerBuilders_func());
    }

    public static IApplicationBuilder UseQuartzJob<TJob>(
      this IApplicationBuilder app,
      IEnumerable<TriggerBuilder> triggerBuilders)
      where TJob : class, IJob
    {
        return app.UseQuartzJob(typeof(TJob), triggerBuilders);
    }

    public static IApplicationBuilder UseQuartzJob(
         this IApplicationBuilder app, Type t,
         TriggerBuilder triggerBuilder)
    {
        return app.UseQuartzJob(t, new TriggerBuilder[] { triggerBuilder });
    }

    public static IApplicationBuilder UseQuartzJob(
          this IApplicationBuilder app, Type t,
         Func<TriggerBuilder> triggerBuilders_func)
    {
        var lst = new List<TriggerBuilder>();
        var tb = triggerBuilders_func?.Invoke();
        if (tb != null)
        {
            lst.Add(tb);
        }
        return app.UseQuartzJob(t, lst);
    }

    public static IServiceCollection UseQuartzJob(this IServiceCollection app, Type t, Func<TriggerBuilder> triggerBuilders_func)
    {
        var lst = new List<TriggerBuilder>();
        var tb = triggerBuilders_func?.Invoke();
        if (tb != null)
        {
            lst.Add(tb);
        }
        return app.UseQuartzJob(t, lst);
    }

    public static IApplicationBuilder UseQuartzJob(
       this IApplicationBuilder app, Type t,
       Func<IEnumerable<TriggerBuilder>> triggerBuilders_func)
    {
        return app.UseQuartzJob(t, triggerBuilders_func());
    }

    public static IApplicationBuilder UseQuartzJob(this IApplicationBuilder app, Type t, IEnumerable<TriggerBuilder> triggerBuilders)
    {
        var _scheduleJobs = app.ApplicationServices.GetService<IEnumerable<IScheduleJob>>();
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

    public static IServiceCollection UseQuartzJob(this IServiceCollection app, Type t, IEnumerable<TriggerBuilder> triggerBuilders)
    {
        var _scheduleJobs = app.GetRequiredService<IEnumerable<IScheduleJob>>();
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
