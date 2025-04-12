using Microsoft.AspNetCore.Builder;
using Quartz;
using Quartz.Spi;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartzAdmin(
            this IServiceCollection services,
            //Action<QuartzAdminOptions> configureOptions = null,
            //Action<QuartzAdminAuthenticationOptions> configureAuthenticationOptions = null,
            Action<NameValueCollection> stdSchedulerFactoryOptions = null,
            Func<List<Assembly>> jobsasmlist = null)
        {
            //var options = new QuartzAdminOptions();
            //configureOptions?.Invoke(options);
            //services.AddSingleton(options);

            //var authenticationOptions = new QuartzAdminAuthenticationOptions();
            //configureAuthenticationOptions?.Invoke(authenticationOptions);




            //services.AddSingleton(authenticationOptions);
            //if (authenticationOptions.AccessRequirement != QuartzAdminAuthenticationOptions.SimpleAccessRequirement.AllowAnonymous)
            //{
            //    services
            //        .AddAuthentication(authenticationOptions.AuthScheme)
            //        .AddCookie(authenticationOptions.AuthScheme, cfg =>
            //        {
            //            cfg.Cookie.Name = $"sq_authenticationOptions.AuthScheme";
            //            cfg.LoginPath = $"{options.VirtualPathRoot}/Authenticate/Login";
            //            cfg.AccessDeniedPath = $"{options.VirtualPathRoot}/Authenticate/Login";
            //            cfg.ExpireTimeSpan = TimeSpan.FromDays(7);
            //            cfg.SlidingExpiration = true;
            //        });
            //}
            //services.AddAuthorization(opts =>
            //    {
            //        opts.AddPolicy(QuartzAdminAuthenticationOptions.AuthorizationPolicyName, builder =>
            //        {
            //            builder.AddRequirements(new QuartzAdminDefaultAuthorizationRequirement(authenticationOptions.AccessRequirement));
            //        });
            //    });
            //services.AddScoped<IAuthorizationHandler, QuartzAdminDefaultAuthorizationHandler>();


            //services.UseQuartzHostedService(stdSchedulerFactoryOptions);
            services.AddSingleton<IJobFactory, ServiceCollectionJobFactory>();
            var bb=new JobRegistrator(services);
            var types = JobsListHelper.GetQuartzAdminJobs(jobsasmlist?.Invoke());
            types.ForEach(t =>
            {
                var so = t.GetCustomAttribute<QuartzTriggerAttribute>();
                services.AddQuartzJob(t, so.Identity ?? t.Name, so.Desciption ?? t.FullName);
            });

            return services;
        }
    }
}
