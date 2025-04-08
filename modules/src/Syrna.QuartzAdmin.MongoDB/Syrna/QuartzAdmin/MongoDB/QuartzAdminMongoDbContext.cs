using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace Syrna.QuartzAdmin.MongoDB
{
    [ConnectionStringName(QuartzAdminDbProperties.ConnectionStringName)]
    public class QuartzAdminMongoDbContext : AbpMongoDbContext, IQuartzAdminMongoDbContext
    {
        /* Add mongo collections here. Example:
         * public IMongoCollection<Question> Questions => Collection<Question>();
         */

        protected override void CreateModel(IMongoModelBuilder modelBuilder)
        {
            base.CreateModel(modelBuilder);

            modelBuilder.ConfigureQuartzAdmin();
        }
    }
}