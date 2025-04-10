using Quartz;

namespace Syrna.QuartzAdmin.Scheduler
{
    /// <summary>
    /// Statistic details for a <see cref="IScheduler"/>
    /// </summary>
    public sealed class SchedulerStatisticsDetails
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="metaData">Metadata from a scheduler.</param>
        public static SchedulerStatisticsDetails Create(SchedulerMetaData metaData)
        {
            var result = new SchedulerStatisticsDetails();
            result.NumberOfJobsExecuted = metaData.NumberOfJobsExecuted;
            return result;
        }

        /// <summary>
        /// The number of jobs a scheduler has executed.
        /// </summary>
        public int NumberOfJobsExecuted { get; set; }
    }
}
