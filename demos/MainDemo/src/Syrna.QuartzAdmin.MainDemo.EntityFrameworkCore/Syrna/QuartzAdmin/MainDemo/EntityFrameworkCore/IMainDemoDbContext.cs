using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Syrna.QuartzAdmin;

namespace Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore
{
    [ConnectionStringName(QuartzAdminDbProperties.ConnectionStringName)]
    public interface IMainDemoDbContext : IEfCoreDbContext
    {
    }
}
