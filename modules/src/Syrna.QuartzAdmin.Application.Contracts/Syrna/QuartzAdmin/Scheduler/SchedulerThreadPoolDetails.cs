using Quartz;
using Quartz.Util;
using System;

namespace Syrna.QuartzAdmin.Scheduler
{
    /// <summary>
    /// Model for the <see cref="IScheduler"/> Thread pool settings.
    /// </summary>
    [Serializable]
    public class SchedulerThreadPoolDetails
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="metaData">The <see cref="IScheduler"/> meta data.</param>
        public static SchedulerThreadPoolDetails Create(SchedulerMetaData metaData)
        {
            var result = new SchedulerThreadPoolDetails();
            result.Type = metaData.ThreadPoolType.AssemblyQualifiedNameWithoutVersion();
            result.Size = metaData.ThreadPoolSize;
            return result;
        }

        /// <summary>
        /// Type of thread pool.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Number of threads in the pool.
        /// </summary>
        public int Size { get; private set; }
    }
}
