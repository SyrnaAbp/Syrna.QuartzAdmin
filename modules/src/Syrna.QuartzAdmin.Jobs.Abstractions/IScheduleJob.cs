using System.Collections.Generic;
using Quartz;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public interface IScheduleJob
    {
        IJobDetail JobDetail { get; set; }
        IEnumerable<ITrigger> Triggers { get; set; }
    }
}