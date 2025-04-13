using System;
namespace Syrna.QuartzAdmin
{
    public class QuartzAdminCoreOptions
    {
        /// <summary>
        /// <para>Assembly files that contain IJob or IJobUI implementation use for creating schedule.</para>
        /// <para>Ex. Quartz.Jobs</para>
        /// <para>Or Jobs/Quartz.Jobs - if this file is under Job folder</para>
        /// </summary>
        public string[] AllowedJobAssemblyFiles { get; set; }
       
        /// <summary>
        /// <para>Job types that are not allowed to be used for creating new Jobs using UI.</para>
        /// <para>Ex. Quartz.Job.NativeJob</para>
        /// </summary>
        public string[] DisallowedJobTypes { get; set; }

        public string HousekeepingCronSchedule { get; set; } = "0 0 1 * * ?";

        public int ExecutionLogsDaysToKeep { get; set; } = 21;
    }
}
