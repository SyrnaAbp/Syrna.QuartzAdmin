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
                x.ConfigureByConvention();
                x.ToTable($"{options.TablePrefix}ExecutionHistories", options.Schema);

                x.OwnsOne(l => l.ExecutionHistoryDetail, e =>
                {
                    e.ToTable($"{options.TablePrefix}ExecutionHistoryDetail", options.Schema);
                    e.WithOwner().HasForeignKey(x => x.LogId);
                });

                x.HasIndex(l => l.FireInstanceId).IsUnique();

                // for housekeeping or system log display
                x.HasIndex(l => new { l.DateAddedUtc, l.LogType });

                // joining with job
                x.HasIndex(l => new { l.TriggerName, l.TriggerGroup, l.JobName, l.JobGroup, l.DateAddedUtc });

                x.Property(e => e.LogType).HasConversion<string>();
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
