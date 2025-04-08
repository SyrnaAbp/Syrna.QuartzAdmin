using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace Syrna.QuartzAdmin.MongoDB
{
    [ConnectionStringName(QuartzAdminDbProperties.ConnectionStringName)]
    public interface IQuartzAdminMongoDbContext : IAbpMongoDbContext
    {
        /* Define mongo collections here. Example:
         * IMongoCollection<Question> Questions { get; }
         */
    }
}
