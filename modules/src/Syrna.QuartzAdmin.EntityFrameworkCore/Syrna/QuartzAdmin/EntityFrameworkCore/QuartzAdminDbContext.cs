using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Syrna.QuartzAdmin.EntityFrameworkCore
{
    [ConnectionStringName(QuartzAdminDbProperties.ConnectionStringName)]
    public class QuartzAdminDbContext : AbpDbContext<QuartzAdminDbContext>, IQuartzAdminDbContext
    {
        /* Add DbSet for each Aggregate Root here. Example:
         * public DbSet<Question> Questions { get; set; }
         */
        public QuartzAdminDbContext(DbContextOptions<QuartzAdminDbContext> options) 
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureQuartzAdmin();
        }
    }
}
