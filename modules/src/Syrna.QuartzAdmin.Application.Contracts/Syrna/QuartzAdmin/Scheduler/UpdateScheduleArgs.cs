using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Triggers;

namespace Syrna.QuartzAdmin.Scheduler;

public class UpdateScheduleArgs
{
    public Key OldJobKey { get; set; }
    public Key OldTriggerKey { get; set; }
    public JobDetailModel NewJobModel { get; set; }
    public TriggerDetailModel NewTriggerModel { get; set; }
}