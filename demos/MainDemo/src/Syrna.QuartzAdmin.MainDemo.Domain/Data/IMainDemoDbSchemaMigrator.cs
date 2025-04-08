using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.MainDemo.Data;

public interface IMainDemoDbSchemaMigrator
{
    Task MigrateAsync();
}
