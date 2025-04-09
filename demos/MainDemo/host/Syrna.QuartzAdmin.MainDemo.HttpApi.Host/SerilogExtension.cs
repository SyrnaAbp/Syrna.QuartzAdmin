using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;

namespace Syrna.QuartzAdmin.MainDemo;

public static class SerilogExtension
{
    public static IHostBuilder AddSerilog(this IHostBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment}.json", true)
            .Build();

        if (environment == null)
        {
            return null;
        }

        var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
        if (applicationName == null)
        {
            return null;
        }

        var appName = configuration["Serilog:Properties:Application"];
        appName ??= "QuartzAdmin";

        var elasticUri = configuration["ElasticConfiguration:Uri"];
        elasticUri ??= "https://127.0.0.1:9200/";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ApplicationName", $"{applicationName} - {environment}")
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers().WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
            .WriteTo.Async(writeTo => writeTo.Console(new JsonFormatter()))
            .WriteTo.Async(writeTo => writeTo.Debug(new RenderedCompactJsonFormatter()))
            .WriteTo.Async(writeTo => writeTo.File($@"c:\logs\{appName}\log-.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Async(writeTo => writeTo
                .Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        TypeName = null,
                        AutoRegisterTemplate = true,
                        IndexFormat = $"{applicationName.ToLower()}-{environment}-{DateTime.UtcNow:yyyy-MM-dd}",
                        BatchAction = ElasticOpType.Create,
                        CustomFormatter = new ElasticsearchJsonFormatter(),
                        OverwriteTemplate = true,
                        DetectElasticsearchVersion = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                        NumberOfReplicas = 1,
                        NumberOfShards = 2,
                        FailureCallback = (l) =>
                        {
                            Console.WriteLine("Unable to submit event " + l.MessageTemplate);
                        },
                        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback |
                                       EmitEventFailureHandling.ThrowException,
                        ModifyConnectionSettings = x => x
                            .CertificateFingerprint(configuration["ElasticConfiguration:Fingerprint"])
                            .BasicAuthentication(
                            configuration["ElasticConfiguration:UserName"],
                            configuration["ElasticConfiguration:Password"]
                        ),
                    }
                )
            )
        //.WriteTo.Async(writeTo => writeTo.Seq(serverUrl: "seq_server-url", apiKey: "seq-api-keu"))
        .CreateLogger();

        builder.ConfigureLogging(configureLogging => { configureLogging.ClearProviders(); })
            .UseSerilog(Log.Logger, true);
        return builder;
    }
}