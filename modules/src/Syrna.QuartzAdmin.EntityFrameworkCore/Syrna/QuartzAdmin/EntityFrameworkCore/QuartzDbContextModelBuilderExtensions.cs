using Microsoft.EntityFrameworkCore;
using Syrna.QuartzAdmin.Quartz;
using System;
using Volo.Abp;

namespace Syrna.QuartzAdmin.EntityFrameworkCore;

public static class QuartzDbContextModelBuilderExtensions
{
    public static void ConfigureQuartz(
        this ModelBuilder builder,
       Action<QuartzAdminModelBuilderConfigurationOptions> optionsAction = null)
    {
        Check.NotNull(builder, nameof(builder));

        var options = new QuartzAdminModelBuilderConfigurationOptions(
            QuartzDbProperties.DbTablePrefix,
            QuartzDbProperties.DbSchema
        );

        optionsAction?.Invoke(options);

        builder.Entity<QuartzBlobTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}blob_triggers", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("trigger_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.BlobData)
              .HasColumnName("blob_data")
              .HasColumnType("bytea");

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.BlobTriggers)
              .HasForeignKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzCalendar>(x =>
        {
            x.ToTable($"{options.TablePrefix}calendars", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.CalendarName });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.CalendarName)
              .HasColumnName("calendar_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.Calendar)
              .HasColumnName("calendar")
              .HasColumnType("bytea")
              .IsRequired();
        });

        builder.Entity<QuartzCronTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}cron_triggers", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("trigger_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.CronExpression)
              .HasColumnName("cron_expression")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TimeZoneId)
              .HasColumnName("time_zone_id")
              .HasColumnType("text");

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.CronTriggers)
              .HasForeignKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzFiredTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}fired_triggers", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.EntryId });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.EntryId)
              .HasColumnName("entry_id")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("trigger_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.InstanceName)
              .HasColumnName("instance_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.FiredTime)
              .HasColumnName("fired_time")
              .HasColumnType("bigint")
              .IsRequired();

            x.Property(p => p.ScheduledTime)
              .HasColumnName("sched_time")
              .HasColumnType("bigint")
              .IsRequired();

            x.Property(p => p.Priority)
              .HasColumnName("priority")
              .HasColumnType("integer")
              .IsRequired();

            x.Property(p => p.State)
              .HasColumnName("state")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.JobName)
              .HasColumnName("job_name")
              .HasColumnType("text");

            x.Property(p => p.JobGroup)
              .HasColumnName("job_group")
              .HasColumnType("text");

            x.Property(p => p.IsNonConcurrent)
              .HasColumnName("is_nonconcurrent")
              .HasColumnType("bool")
              .IsRequired();

            x.Property(p => p.RequestsRecovery)
              .HasColumnName("requests_recovery")
              .HasColumnType("bool");

            x.HasIndex(p => p.TriggerName)
              .HasDatabaseName($"idx_{options.TablePrefix}ft_trig_name");

            x.HasIndex(p => p.TriggerGroup)
              .HasDatabaseName($"idx_{options.TablePrefix}ft_trig_group");

            x.HasIndex(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .HasDatabaseName($"idx_{options.TablePrefix}ft_trig_nm_gp");

            x.HasIndex(p => p.InstanceName)
              .HasDatabaseName($"idx_{options.TablePrefix}ft_trig_inst_name");

            x.HasIndex(p => p.JobName)
              .HasDatabaseName($"idx_{options.TablePrefix}ft_job_name");

            x.HasIndex(p => p.JobGroup)
              .HasDatabaseName($"idx_{options.TablePrefix}ft_job_group");

            x.HasIndex(p => p.RequestsRecovery)
              .HasDatabaseName($"idx_{options.TablePrefix}ft_job_req_recovery");
        });

        builder.Entity<QuartzJobDetail>(x =>
        {
            x.ToTable($"{options.TablePrefix}job_details", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.JobName, p.JobGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.JobName)
              .HasColumnName("job_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.JobGroup)
              .HasColumnName("job_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.Description)
              .HasColumnName("description")
              .HasColumnType("text");

            x.Property(p => p.JobClassName)
              .HasColumnName("job_class_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.IsDurable)
              .HasColumnName("is_durable")
              .HasColumnType("bool")
              .IsRequired();

            x.Property(p => p.IsNonConcurrent)
              .HasColumnName("is_nonconcurrent")
              .HasColumnType("bool")
              .IsRequired();

            x.Property(p => p.IsUpdateData)
              .HasColumnName("is_update_data")
              .HasColumnType("bool")
              .IsRequired();

            x.Property(p => p.RequestsRecovery)
              .HasColumnName("requests_recovery")
              .HasColumnType("bool")
              .IsRequired();

            x.Property(p => p.JobData)
              .HasColumnName("job_data")
              .HasColumnType("bytea");

            x.HasIndex(p => p.RequestsRecovery)
              .HasDatabaseName($"idx_{options.TablePrefix}j_req_recovery");
        });

        builder.Entity<QuartzLock>(x =>
        {
            x.ToTable($"{options.TablePrefix}locks", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.LockName });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.LockName)
              .HasColumnName("lock_name")
              .HasColumnType("text")
              .IsRequired();
        });

        builder.Entity<QuartzPausedTriggerGroup>(x =>
        {
            x.ToTable($"{options.TablePrefix}paused_trigger_grps", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();
        });

        builder.Entity<QuartzSchedulerState>(x =>
        {
            x.ToTable($"{options.TablePrefix}scheduler_state", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.InstanceName });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.InstanceName)
              .HasColumnName("instance_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.LastCheckInTime)
              .HasColumnName("last_checkin_time")
              .HasColumnType("bigint")
              .IsRequired();

            x.Property(p => p.CheckInInterval)
              .HasColumnName("checkin_interval")
              .HasColumnType("bigint")
              .IsRequired();
        });

        builder.Entity<QuartzSimplePropertyTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}simprop_triggers", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("trigger_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.StringProperty1)
              .HasColumnName("str_prop_1")
              .HasColumnType("text");

            x.Property(p => p.StringProperty2)
              .HasColumnName("str_prop_2")
              .HasColumnType("text");

            x.Property(p => p.StringProperty3)
              .HasColumnName("str_prop_3")
              .HasColumnType("text");

            x.Property(p => p.IntegerProperty1)
              .HasColumnName("int_prop_1")
              .HasColumnType("integer");

            x.Property(p => p.IntegerProperty2)
              .HasColumnName("int_prop_2")
              .HasColumnType("integer");

            x.Property(p => p.LongProperty1)
              .HasColumnName("long_prop_1")
              .HasColumnType("bigint");

            x.Property(p => p.LongProperty2)
              .HasColumnName("long_prop_2")
              .HasColumnType("bigint");

            x.Property(p => p.DecimalProperty1)
              .HasColumnName("dec_prop_1")
              .HasColumnType("numeric");

            x.Property(p => p.DecimalProperty2)
              .HasColumnName("dec_prop_2")
              .HasColumnType("numeric");

            x.Property(p => p.BooleanProperty1)
              .HasColumnName("bool_prop_1")
              .HasColumnType("bool");

            x.Property(p => p.BooleanProperty2)
              .HasColumnName("bool_prop_2")
              .HasColumnType("bool");

            x.Property(p => p.TimeZoneId)
              .HasColumnName("time_zone_id")
              .HasColumnType("text");

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.SimplePropertyTriggers)
              .HasForeignKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzSimpleTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}simple_triggers", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("trigger_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.RepeatCount)
              .HasColumnName("repeat_count")
              .HasColumnType("bigint")
              .IsRequired();

            x.Property(p => p.RepeatInterval)
              .HasColumnName("repeat_interval")
              .HasColumnType("bigint")
              .IsRequired();

            x.Property(p => p.TimesTriggered)
              .HasColumnName("times_triggered")
              .HasColumnType("bigint")
              .IsRequired();

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.SimpleTriggers)
              .HasForeignKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}triggers", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("sched_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("trigger_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("trigger_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.JobName)
              .HasColumnName("job_name")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.JobGroup)
              .HasColumnName("job_group")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.Description)
              .HasColumnName("description")
              .HasColumnType("text");

            x.Property(p => p.NextFireTime)
              .HasColumnName("next_fire_time")
              .HasColumnType("bigint");

            x.Property(p => p.PreviousFireTime)
              .HasColumnName("prev_fire_time")
              .HasColumnType("bigint");

            x.Property(p => p.Priority)
              .HasColumnName("priority")
              .HasColumnType("integer");

            x.Property(p => p.TriggerState)
              .HasColumnName("trigger_state")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.TriggerType)
              .HasColumnName("trigger_type")
              .HasColumnType("text")
              .IsRequired();

            x.Property(p => p.StartTime)
              .HasColumnName("start_time")
              .HasColumnType("bigint")
              .IsRequired();

            x.Property(p => p.EndTime)
              .HasColumnName("end_time")
              .HasColumnType("bigint");

            x.Property(p => p.CalendarName)
              .HasColumnName("calendar_name")
              .HasColumnType("text");

            x.Property(p => p.MisfireInstruction)
              .HasColumnName("misfire_instr")
              .HasColumnType("smallint");

            x.Property(p => p.JobData)
              .HasColumnName("job_data")
              .HasColumnType("bytea");

            x.HasOne(p => p.JobDetail)
              .WithMany(p => p.Triggers)
              .HasForeignKey(p => new { p.SchedulerName, p.JobName, p.JobGroup })
              .IsRequired();

            x.HasIndex(p => p.NextFireTime)
              .HasDatabaseName($"idx_{options.TablePrefix}t_next_fire_time");

            x.HasIndex(p => p.TriggerState)
              .HasDatabaseName($"idx_{options.TablePrefix}t_state");

            x.HasIndex(x => new { x.NextFireTime, x.TriggerState })
              .HasDatabaseName($"idx_{options.TablePrefix}t_nft_st");
        });
    }
}
