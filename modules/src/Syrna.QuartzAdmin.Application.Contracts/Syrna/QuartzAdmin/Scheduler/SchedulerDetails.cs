using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Scheduler
{
    /// <summary>
    /// Data model for the scheduler instance details.
    /// </summary>
    [Serializable]
    public class SchedulerDetails
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/>  instance.</param>
        /// <param name="metaData">The <see cref="SchedulerMetaData"/> meta data.</param>
        public static SchedulerDetails Create(IScheduler scheduler, SchedulerMetaData metaData)
        {
            var result = new SchedulerDetails();
            result.Name = scheduler.SchedulerName;
            result.SchedulerInstanceId = scheduler.SchedulerInstanceId;
            result.Status = TranslateStatus(scheduler);
            result.RunningSince = metaData.RunningSince?.LocalDateTime.ToString(CultureInfo.InvariantCulture) ?? "N / A";
            result.QuartzVersion = metaData.Version;
            result.ThreadPool =  SchedulerThreadPoolDetails.Create(metaData);
            result.JobStore =  SchedulerJobStoreDetails.Create(metaData);
            result.Statistics =  SchedulerStatisticsDetails.Create(metaData);
            result.JobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).GetAwaiter().GetResult();
            result.TriggerKeys = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).GetAwaiter().GetResult();
            result.GetJobTriggerPausedGroups(scheduler).GetAwaiter().GetResult();
            return result;
        }

        /// <summary>
        /// List of all triggers configured for the scheduler.
        /// </summary>
        public IReadOnlyCollection<TriggerKey> TriggerKeys { get; set; }
        /// <summary>
        /// List of all jobs configured for the scheduler.
        /// </summary>
        public IReadOnlyCollection<JobKey> JobKeys { get; set; }
        /// <summary>
        /// List of all paused job groups.
        /// </summary>
        public IEnumerable<object> PausedJobGroups { get; set; }
        /// <summary>
        /// List of all paused trigger groups.
        /// </summary>
        public IEnumerable<object> PausedTriggerGroups { get; set; }

        /// <summary>
        /// Instance id for the scheduler.
        /// </summary>
        public string SchedulerInstanceId { get; set; }
        /// <summary>
        /// Name of the scheduler.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Scheduler status.
        /// </summary>
        public SchedulerStatus Status { get; set; }
        /// <summary>
        /// Date stamp when scheduler started.
        /// </summary>
        public string RunningSince { get; set; }
        /// <summary>
        /// Scheduler version.
        /// </summary>
        public string QuartzVersion { get; set; }
        /// <summary>
        /// Number of jobs configured in the scheduler.
        /// </summary>
        public int NumberOfJobs => JobKeys.Count;
        /// <summary>
        /// Number of triggers configured in the scheduler.
        /// </summary>
        public int NumberOfTriggers => TriggerKeys.Count;

        /// <summary>
        /// Scheduler Thread Pool details.
        /// </summary>
        public SchedulerThreadPoolDetails ThreadPool { get; set; }
        /// <summary>
        /// Scheduler JobStore details.
        /// </summary>
        public SchedulerJobStoreDetails JobStore { get; set; }
        /// <summary>
        /// Scheduler Statistics.
        /// </summary>
        public SchedulerStatisticsDetails Statistics { get; set; }


        #region Private helpers
        public static SchedulerStatus TranslateStatus(IScheduler scheduler)
        {
            if (scheduler.IsShutdown)
            {
                return SchedulerStatus.Shutdown;
            }
            if (scheduler.InStandbyMode)
            {
                return SchedulerStatus.Standby;
            }

            return scheduler.IsStarted
                ? SchedulerStatus.Running
                : SchedulerStatus.Unknown;
        }

        private async Task GetJobTriggerPausedGroups(IScheduler scheduler)
        {
            try
            {
                PausedJobGroups = await GetGroupPauseState(
                    await scheduler.GetJobGroupNames(),
                    async x => await scheduler.IsJobGroupPaused(x));
            }
            catch (NotImplementedException) { }

            try
            {
                PausedTriggerGroups = await GetGroupPauseState(
                    await scheduler.GetTriggerGroupNames(),
                    async x => await scheduler.IsTriggerGroupPaused(x));
            }
            catch (NotImplementedException) { }
        }

        private static async Task<IEnumerable<object>> GetGroupPauseState(IEnumerable<string> groups, Func<string, Task<bool>> func)
        {
            var result = new List<object>();

            foreach (var name in groups.OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase))
                result.Add(new { Name = name, IsPaused = await func(name) });

            return result;
        }
        #endregion
    }
}
