using Quartz;
using Quartz.Util;

namespace Syrna.QuartzAdmin.Scheduler
{
    /// <summary>
    /// Model for Scheduler Job store details.
    /// </summary>
    public sealed class SchedulerJobStoreDetails
    {
        /// <summary>
        /// Statistic details for a <see cref="IScheduler"/>
        /// </summary>
        public static SchedulerJobStoreDetails Create(SchedulerMetaData metaData)
        {
            var result = new SchedulerJobStoreDetails();
            result.Type = metaData.JobStoreType.AssemblyQualifiedNameWithoutVersion();
            result.Clustered = metaData.JobStoreClustered;
            result.Persistent = metaData.JobStoreSupportsPersistence;
            return result;
        }

        /// <summary>
        /// Job Store Type.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Is Job Stored clustered.
        /// </summary>
        public bool Clustered { get; set; }
        /// <summary>
        /// Is Job Store Persistent
        /// </summary>
        public bool Persistent { get; set; }
    }
}
