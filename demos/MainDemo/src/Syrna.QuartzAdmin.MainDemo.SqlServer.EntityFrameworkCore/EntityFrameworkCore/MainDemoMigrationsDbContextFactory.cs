using System.IO;
using Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Syrna.QuartzAdmin.MainDemo.SqlServer.EntityFrameworkCore;

/* This class is needed for EF Core console commands
     * (like Add-Migration and Update-Database commands) */
public class MainDemoMigrationsDbContextFactory : IDesignTimeDbContextFactory<MainDemoMigrationsDbContext>
{
    public MainDemoMigrationsDbContext CreateDbContext(string[] args)
    {
        MainDemoEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<MainDemoMigrationsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new MainDemoMigrationsDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Syrna.QuartzAdmin.MainDemo.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
