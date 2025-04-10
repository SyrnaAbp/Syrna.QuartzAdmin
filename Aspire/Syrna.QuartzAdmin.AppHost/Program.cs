using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var nss = builder.AddConnectionString("Default");

var apiService = builder.AddProject<Projects.Syrna_QuartzAdmin_MainDemo_HttpApi_Host>("API")
    .WithReference(nss);

builder.AddProject<Projects.Syrna_QuartzAdmin_MainDemo_AuthServer>("AuthServer")
    .WithReference(apiService)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Syrna_QuartzAdmin_MainDemo_Blazor_Host_Client>("BlazorHostClient")
    .WithReference(apiService)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Syrna_QuartzAdmin_MainDemo_Blazor_Host>("BlazorHost")
    .WithReference(apiService)
    .WithExternalHttpEndpoints();

//builder.AddProject<Projects.Syrna_QuartzAdmin_MainDemo_DbMigrator>("DbMigrator")
//    .WithReference(nss);

builder.Build().Run();
