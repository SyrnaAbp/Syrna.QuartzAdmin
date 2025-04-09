using Serilog;
using Syrna.QuartzAdmin.MainDemo;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.AddSerilog();
    Log.Information("Starting Syrna.QuartzAdmin.MainDemo.HttpApi.Host.");
    //builder.AddSqlServerDbContext<AccountingPreDbContext>("Default");
    builder.Host
        .AddAppSettingsSecretsJson()
        .UseSerilog()
        .UseAutofac()
        ;
    await builder.AddApplicationAsync<MainDemoHttpApiHostModule>();
    var app = builder.Build();
    await app.InitializeApplicationAsync();
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    if (ex is HostAbortedException)
    {
        throw;
    }

    Log.Fatal(ex, "Host terminated unexpectedly!");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
