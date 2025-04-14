using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Syrna.QuartzAdmin.Jobs.Abstractions;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartzAdminJobs(this IServiceCollection services)
        {
            services.TryAddTransient<IDataMapValueResolver, DataMapValueResolver>();

            return services;
        }
    }
}

