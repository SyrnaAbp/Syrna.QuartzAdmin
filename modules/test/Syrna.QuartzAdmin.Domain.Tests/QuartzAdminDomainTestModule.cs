using Syrna.QuartzAdmin.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Syrna.QuartzAdmin
{
    /* Domain tests are configured to use the EF Core provider.
     * You can switch to MongoDB, however your domain tests should be
     * database independent anyway.
     */
    [DependsOn(
        typeof(QuartzAdminEntityFrameworkCoreTestModule)
        )]
    public class QuartzAdminDomainTestModule : AbpModule
    {
        
    }
}
