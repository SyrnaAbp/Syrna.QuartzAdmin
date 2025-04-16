using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Triggers;

namespace Syrna.QuartzAdmin.Scheduler
{
    public class CreateScheduleArgs
    {
        public JobDetailModel JobDetailModel { get; set; }
        public TriggerDetailModel TriggerDetailModel { get; set; }
    }
}