using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Syrna.QuartzAdmin.EntityFrameworkCore
{
    [ConnectionStringName(QuartzAdminDbProperties.ConnectionStringName)]
    public interface IQuartzAdminDbContext : IEfCoreDbContext
    {
        /* Add DbSet for each Aggregate Root here. Example:
         * DbSet<Question> Questions { get; }
         */
    }
}
