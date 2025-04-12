using DeviceDetectorNET.Parser.Device;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace Syrna.QuartzAdmin.AspNetCore;

[DependsOn(
    typeof(AbpMultiTenancyModule),
    typeof(AbpAspNetCoreModule)
)]
public class QuartzAdminAspNetCoreModule : AbpModule
{
    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var quartzOptions = configuration.GetSection("Quartz");
        if (quartzOptions != null)
        {
            if (quartzOptions["Enabled"]?.ToLower() == "true")
            {
            }
        }
    }
}
