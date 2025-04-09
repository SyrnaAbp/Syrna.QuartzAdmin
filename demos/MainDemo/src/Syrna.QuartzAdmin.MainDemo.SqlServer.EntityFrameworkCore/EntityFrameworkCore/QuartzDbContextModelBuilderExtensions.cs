using Microsoft.EntityFrameworkCore;
using Syrna.QuartzAdmin.EntityFrameworkCore;
using Syrna.QuartzAdmin.Quartz;
using System;
using Volo.Abp;

namespace Syrna.QuartzAdmin.MainDemo.SqlServer.EntityFrameworkCore;

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
            x.ToTable($"{options.TablePrefix}BLOB_TRIGGERS", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("TRIGGER_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.BlobData)
              .HasColumnName("BLOB_DATA");

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.BlobTriggers)
              .HasForeignKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzCalendar>(x =>
        {
            x.ToTable($"{options.TablePrefix}CALENDARS", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.CalendarName });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.CalendarName)
              .HasColumnName("CALENDAR_NAME")
              .HasMaxLength(200)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.Calendar)
              .HasColumnName("CALENDAR")
              .IsRequired();
        });

        builder.Entity<QuartzCronTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}CRON_TRIGGERS", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("TRIGGER_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.CronExpression)
              .HasColumnName("CRON_EXPRESSION")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TimeZoneId)
              .HasColumnName("TIME_ZONE_ID")
              .HasMaxLength(120)
              .IsUnicode();

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.CronTriggers)
              .HasForeignKey(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzFiredTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}FIRED_TRIGGERS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.EntryId });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.EntryId)
              .HasColumnName("ENTRY_ID")
              .HasMaxLength(140)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("TRIGGER_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.InstanceName)
              .HasColumnName("INSTANCE_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.FiredTime)
              .HasColumnName("FIRED_TIME")
              .IsRequired();

            x.Property(p => p.ScheduledTime)
              .HasColumnName("SCHED_TIME")
              .IsRequired();

            x.Property(p => p.Priority)
              .HasColumnName("PRIORITY")
              .IsRequired();

            x.Property(p => p.State)
              .HasColumnName("STATE")
              .HasMaxLength(16)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.JobName)
              .HasColumnName("JOB_NAME")
              .HasMaxLength(150)
              .IsUnicode();

            x.Property(p => p.JobGroup)
              .HasColumnName("JOB_GROUP")
              .HasMaxLength(150)
              .IsUnicode();

            x.Property(p => p.IsNonConcurrent)
              .HasColumnName("IS_NONCONCURRENT");   

            x.Property(p => p.RequestsRecovery)
              .HasColumnName("REQUESTS_RECOVERY");

            x.HasIndex(p => p.TriggerName)
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_TRIG_NAME");

            x.HasIndex(p => p.TriggerGroup)
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_TRIG_GROUP");

            x.HasIndex(p => new { p.SchedulerName, p.TriggerName, p.TriggerGroup })
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_TRIG_NM_GP");

            x.HasIndex(p => p.InstanceName)
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_TRIG_INST_NAME");

            x.HasIndex(p => p.JobName)
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_JOB_NAME");

            x.HasIndex(p => p.JobGroup)
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_JOB_GROUP");

            x.HasIndex(p => p.RequestsRecovery)
              .HasDatabaseName($"IDX_{options.TablePrefix}FT_JOB_REQ_RECOVERY");
        });

        builder.Entity<QuartzJobDetail>(x =>
        {
            x.ToTable($"{options.TablePrefix}JOB_DETAILS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.JobName, x.JobGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.JobName)
              .HasColumnName("JOB_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.JobGroup)
              .HasColumnName("JOB_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.Description)
              .HasColumnName("DESCRIPTION")
              .HasMaxLength(250)
              .IsUnicode();

            x.Property(p => p.JobClassName)
              .HasColumnName("JOB_CLASS_NAME")
              .HasMaxLength(250)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.IsDurable)
              .HasColumnName("IS_DURABLE")
              .IsRequired();

            x.Property(p => p.IsNonConcurrent)
              .HasColumnName("IS_NONCONCURRENT")
              .IsRequired();

            x.Property(p => p.IsUpdateData)
              .HasColumnName("IS_UPDATE_DATA")
              .IsRequired();

            x.Property(p => p.RequestsRecovery)
              .HasColumnName("REQUESTS_RECOVERY")
              .IsRequired();

            x.Property(p => p.JobData)
              .HasColumnName("JOB_DATA");

            x.HasIndex(p => p.RequestsRecovery)
              .HasDatabaseName($"IDX_{options.TablePrefix}J_REQ_RECOVERY");
        });

        builder.Entity<QuartzLock>(x =>
        {
            x.ToTable($"{options.TablePrefix}LOCKS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.LockName });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.LockName)
              .HasColumnName("LOCK_NAME")
              .HasMaxLength(40)
              .IsUnicode()
              .IsRequired();
        });

        builder.Entity<QuartzPausedTriggerGroup>(x =>
        {
            x.ToTable($"{options.TablePrefix}PAUSED_TRIGGER_GRPS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();
        });

        builder.Entity<QuartzSchedulerState>(x =>
        {
            x.ToTable($"{options.TablePrefix}SCHEDULER_STATE", options.Schema);

            x.HasKey(p => new { p.SchedulerName, p.InstanceName });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.InstanceName)
              .HasColumnName("INSTANCE_NAME")
              .HasMaxLength(200)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.LastCheckInTime)
              .HasColumnName("LAST_CHECKIN_TIME")
              .IsRequired();

            x.Property(p => p.CheckInInterval)
              .HasColumnName("CHECKIN_INTERVAL")
              .IsRequired();
        });

        builder.Entity<QuartzSimplePropertyTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}SIMPROP_TRIGGERS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("TRIGGER_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.StringProperty1)
              .HasColumnName("STR_PROP_1")
              .HasMaxLength(512)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.StringProperty2)
              .HasColumnName("STR_PROP_2")
              .HasMaxLength(512)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.StringProperty3)
              .HasColumnName("STR_PROP_3")
              .HasMaxLength(512)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.IntegerProperty1)
              .HasColumnName("INT_PROP_1");

            x.Property(p => p.IntegerProperty2)
              .HasColumnName("INT_PROP_2");

            x.Property(p => p.LongProperty1)
              .HasColumnName("LONG_PROP_1");

            x.Property(p => p.LongProperty2)
              .HasColumnName("LONG_PROP_2");

            x.Property(p => p.DecimalProperty1)
              .HasColumnName("DEC_PROP_1")
              .HasColumnType("numeric(13,4)");

            x.Property(p => p.DecimalProperty2)
              .HasColumnName("DEC_PROP_2")
              .HasColumnType("numeric(13,4)");

            x.Property(p => p.BooleanProperty1)
              .HasColumnName("BOOL_PROP_1");

            x.Property(p => p.BooleanProperty2)
              .HasColumnName("BOOL_PROP_2");

            x.Property(p => p.TimeZoneId)
              .HasColumnName("TIME_ZONE_ID")
              .HasMaxLength(80)
              .IsUnicode();

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.SimplePropertyTriggers)
              .HasForeignKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzSimpleTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}SIMPLE_TRIGGERS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("TRIGGER_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.RepeatCount)
              .HasColumnName("REPEAT_COUNT")
              .IsRequired();

            x.Property(p => p.RepeatInterval)
              .HasColumnName("REPEAT_INTERVAL")
              .IsRequired();

            x.Property(p => p.TimesTriggered)
              .HasColumnName("TIMES_TRIGGERED")
              .IsRequired();

            x.HasOne(p => p.Trigger)
              .WithMany(p => p.SimpleTriggers)
              .HasForeignKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup })
              .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuartzTrigger>(x =>
        {
            x.ToTable($"{options.TablePrefix}TRIGGERS", options.Schema);

            x.HasKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup });

            x.Property(p => p.SchedulerName)
              .HasColumnName("SCHED_NAME")
              .HasMaxLength(120)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerName)
              .HasColumnName("TRIGGER_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerGroup)
              .HasColumnName("TRIGGER_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.JobName)
              .HasColumnName("JOB_NAME")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.JobGroup)
              .HasColumnName("JOB_GROUP")
              .HasMaxLength(150)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.Description)
              .HasColumnName("DESCRIPTION")
              .HasMaxLength(250)
              .IsUnicode();

            x.Property(p => p.NextFireTime)
              .HasColumnName("NEXT_FIRE_TIME");

            x.Property(p => p.PreviousFireTime)
              .HasColumnName("PREV_FIRE_TIME");

            x.Property(p => p.Priority)
              .HasColumnName("PRIORITY");

            x.Property(p => p.TriggerState)
              .HasColumnName("TRIGGER_STATE")
              .HasMaxLength(16)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.TriggerType)
              .HasColumnName("TRIGGER_TYPE")
              .HasMaxLength(8)
              .IsUnicode()
              .IsRequired();

            x.Property(p => p.StartTime)
              .HasColumnName("START_TIME")
              .IsRequired();

            x.Property(p => p.EndTime)
              .HasColumnName("END_TIME")
              .HasColumnType("bigint");

            x.Property(p => p.CalendarName)
              .HasColumnName("CALENDAR_NAME")
              .HasMaxLength(200)
              .IsUnicode();

            x.Property(p => p.MisfireInstruction)
              .HasColumnName("MISFIRE_INSTR");

            x.Property(p => p.JobData)
              .HasColumnName("JOB_DATA");

            x.HasOne(p => p.JobDetail)
              .WithMany(p => p.Triggers)
              .HasForeignKey(x => new { x.SchedulerName, x.JobName, x.JobGroup })
              .IsRequired();

            x.HasIndex(p => p.NextFireTime)
              .HasDatabaseName($"IDX_{options.TablePrefix}T_NEXT_FIRE_TIME");

            x.HasIndex(p => p.TriggerState)
              .HasDatabaseName($"IDX_{options.TablePrefix}T_STATE");

            x.HasIndex(x => new { x.NextFireTime, x.TriggerState })
              .HasDatabaseName($"IDX_{options.TablePrefix}T_NFT_ST");
        });
    }
}
