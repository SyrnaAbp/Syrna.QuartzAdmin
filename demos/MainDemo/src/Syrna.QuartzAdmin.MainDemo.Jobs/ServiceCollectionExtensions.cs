using System;
using Microsoft.Extensions.DependencyInjection;

namespace Syrna.QuartzAdmin.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartzAdminJobs(this IServiceCollection services)
        {
            // require to run QuartzAdmin.Jobs.HttpJob
            services.AddHttpClient();
            services.AddHttpClient(Constants.HttpClientIgnoreVerifySsl)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            return Abstractions.ServiceCollectionExtensions.AddQuartzAdminJobs(services);
        }
    }
}

