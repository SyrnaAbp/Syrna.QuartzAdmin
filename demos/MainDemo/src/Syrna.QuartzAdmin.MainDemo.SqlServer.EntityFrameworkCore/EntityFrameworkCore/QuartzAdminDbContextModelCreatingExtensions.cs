using Microsoft.EntityFrameworkCore;
using Syrna.QuartzAdmin.EntityFrameworkCore;
using Syrna.QuartzAdmin.ExecutionHistory;
using System;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Syrna.QuartzAdmin.MainDemo.SqlServer.EntityFrameworkCore
{
    public static class QuartzAdminDbContextModelCreatingExtensions
    {
        public static void ConfigureQuartzAdmin(
            this ModelBuilder builder,
            Action<QuartzAdminModelBuilderConfigurationOptions> optionsAction = null)
        {
            Check.NotNull(builder, nameof(builder));

            var options = new QuartzAdminModelBuilderConfigurationOptions(
                QuartzAdminDbProperties.DbTablePrefix,
                QuartzAdminDbProperties.DbSchema
            );

            optionsAction?.Invoke(options);

            builder.Entity<QuartzExecutionHistory>(x =>
            {
                x.ToTable($"{options.TablePrefix}ExecutionHistories", options.Schema);

                x.ConfigureByConvention();

                // Configure more properties here 
                x.Property(p => p.FireInstanceId)
                    .HasMaxLength(200);
                x.Property(p => p.SchedulerInstanceId)
                    .HasMaxLength(200);
                x.Property(p => p.SchedulerName)
                    .HasMaxLength(200);
                x.Property(p => p.Job)
                    .HasMaxLength(300);
                x.Property(p => p.Trigger)
                    .HasMaxLength(300);
                x.Property(p => p.ScheduledFireTimeUtc)
                    .HasColumnType("datetimeoffset");
                x.Property(p => p.ActualFireTimeUtc)
                    .HasColumnType("datetimeoffset");
                x.Property(p => p.FinishedTimeUtc)
                    .HasColumnType("datetimeoffset");

                x.HasIndex(p => p.FireInstanceId);
            });

            builder.Entity<QuartzJobSummary>(x =>
            {
                x.ToTable($"{options.TablePrefix}JobSummaries", options.Schema);

                x.ConfigureByConvention();

                // Configure more properties here
                x.Property(p => p.SchedulerName).HasMaxLength(200);
            });
        }
    }
}
