# Syrna.QuartzAdmin
Quartz Enterprise Scheduler .NET Admin UI with Blazor, WebApi and Abp Framework

[![ABP version](https://img.shields.io/badge/dynamic/xml?style=flat-square&color=yellow&label=abp&query=%2F%2FProject%2FPropertyGroup%2FVoloAbpPackageVersion&url=https%3A%2F%2Fraw.githubusercontent.com%2FSyrnaAbp%2FSyrna.QuartzAdmin%2Fmaster%2FDirectory.Packages.props)](https://abp.io)
![build and test](https://img.shields.io/github/actions/workflow/status/SyrnaAbp/Syrna.QuartzAdmin/build-all.yml?branch=dev&style=flat-square)
[![NuGet Download](https://img.shields.io/nuget/dt/Syrna.QuartzAdmin.Application.svg?style=flat-square)](https://www.nuget.org/packages/Syrna.QuartzAdmin.Application)
[![NuGet (with prereleases)](https://img.shields.io/nuget/vpre/Syrna.QuartzAdmin.Application.svg?style=flat-square)](https://www.nuget.org/packages/Syrna.QuartzAdmin.Application) 

An abp application module that allows manage quartz scheduling.

## Installation

1. Install the following NuGet packages. ([see how](https://github.com/SyrnaAbp/SyrnaAbpGuide/blob/master/docs/How-To.md#add-nuget-packages))

    * Syrna.QuartzAdmin.Application
    * Syrna.QuartzAdmin.Application.Contracts
    * Syrna.QuartzAdmin.Domain
    * Syrna.QuartzAdmin.Domain.Shared
    * Syrna.QuartzAdmin.EntityFrameworkCore
    * Syrna.QuartzAdmin.HttpApi
    * Syrna.QuartzAdmin.HttpApi.Client
    * Syrna.QuartzAdmin.Web
    * Syrna.QuartzAdmin.Blazor
    * Syrna.QuartzAdmin.Blazor.Server
    * Syrna.QuartzAdmin.Blazor.WebAssembly

1. Add `DependsOn(typeof(QuartzAdminXxxModule))` attribute to configure the module dependencies. ([see how](https://github.com/SyrnaAbp/SyrnaAbpGuide/blob/master/docs/How-To.md#add-module-dependencies))

1. Add `builder.ConfigureQuartzAdmin();` to the `OnModelCreating()` method in **MyProjectMigrationsDbContext.cs**.

1. Add EF Core migrations and update your database. See: [ABP document](https://docs.abp.io/en/abp/latest/Tutorials/Part-1?UI=MVC&DB=EF#add-database-migration).

## Requirements
* .NET 9
* ABP 9.1.1
* Quartz 3.13.0+

## Features
* Add, modify jobs and triggers
* Support Cron, Daily, Simple trigger
* Pause, resume, clone scheduled jobs
* Create custom UI to configure job
* Dynamic variables support
* Monitor currently executing jobs
* Load custom job DLLs through configuration
* Display job execution logs, state, return message and error message
* Filter execution logs
* Store execution logs into any database
  * Build-in support for SQLite, MSSQL and PostgreSQL
* Auto cleanup of old execution logs
  * Configurable logs retention days
* Build-in Jobs
  * HTTP API client job


## Usage
> 1. You must create quartz database. You can find sql mssql script https://github.com/SyrnaAbp/Syrna.QuartzAdmin/blob/dev/demos/MainDemo/src/Syrna.QuartzAdmin.MainDemo.DbMigrator/sqlserver.sql 
> 2. If you will change database, get your sql script from https://github.com/quartznet/quartznet/tree/main/database/tables
> 3. modify your appsettings.json
> 4. More details can be found at [QuartzAdmin](https://github.com/Dolunay/QuartzAdmin)

PostgreSql
   ```
   "ConnectionStrings": {
     "Default": "Host=<db_host>;Port=5432;Database=<db_name>;Username=<db_user>;Password=<db_password>"
   },
   "Quartz": {
     ...
     "quartz.jobStore.driverDelegateType": "Quartz.Impl.AdoJobStore.PostgreSQLDelegate, Quartz",
     ...
     "quartz.dataSource.myDS.provider": "Npgsql"
   },
   "QuartzAdmin": {
     "DataStoreProvider": "PostgreSQL",
   ```
   MsSql
   ```
   "ConnectionStrings": {
    "Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=SyrnaQuartzAdmin;Trusted_Connection=True"
   },
   "Quartz": {
     ...
    "quartz.jobStore.driverDelegateType": "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz",
     ...
     "quartz.dataSource.myDS.provider": "SqlServer"
   },
   "QuartzAdmin": {
     "DataStoreProvider": "SqlServer",
   ```

## Screenshots

![Overview](docs/images/overview.png)
![Schedules](docs/images/schedules.png)
![History](docs/images/history.png)
![Error Details](docs/images/error_details.png)
![Schedules Edit Jobdetails](docs/images/schedules-edit-jobdetails.png)
![Schedules Edit Triggerdetails Cron](docs/images/schedules-edit-triggerdetails-cron.png)
![Schedules Edit Triggerdetails Daily](docs/images/schedules-edit-triggerdetails-daily.png)
![Schedules Edit Triggerdetails Simple](docs/images/schedules-edit-triggerdetails-simple.png)
![History Timeline](docs/images/history-timeline.png)