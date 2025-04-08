using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Syrna.QuartzAdmin.MainDemo.EntityFrameworkCore;

namespace Syrna.QuartzAdmin.MainDemo.PostgreSql.EntityFrameworkCore;

/* This class is needed for EF Core console commands
     * (like Add-Migration and Update-Database commands) */
public class MainDemoMigrationsDbContextFactory : IDesignTimeDbContextFactory<MainDemoMigrationsDbContext>
{
    public MainDemoMigrationsDbContext CreateDbContext(string[] args)
    {
        MainDemoEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<MainDemoMigrationsDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

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
