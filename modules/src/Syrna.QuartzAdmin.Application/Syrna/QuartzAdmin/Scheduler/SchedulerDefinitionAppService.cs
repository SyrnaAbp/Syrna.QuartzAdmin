using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Scheduler
{
    public class SchedulerDefinitionAppService(ISchedulerDefinitionService schedulerDefinitionService) : QuartzAdminAppService, ISchedulerDefinitionAppService
    {
        public Task<List<string>> GetJobTypeNames(bool reload)
        {
            return schedulerDefinitionService.GetJobTypeNames(reload);
        }

        public Task<List<MisfireAction>> GetMisfireActions(TriggerType triggerType)
        {
            return schedulerDefinitionService.GetMisfireActions(triggerType);
        }

        public Task<List<IntervalUnit>> GetTriggerIntervalUnits(TriggerType triggerType)
        {
            return schedulerDefinitionService.GetTriggerIntervalUnits(triggerType);
        }
    }
}

