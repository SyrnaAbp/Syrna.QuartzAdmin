using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Syrna.QuartzAdmin.Authorization;

namespace Syrna.QuartzAdmin.Scheduler
{
    public class SchedulerDefinitionAppService(ISchedulerDefinitionService schedulerDefinitionService) : QuartzAdminAppService, ISchedulerDefinitionAppService
    {
        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public Task<List<string>> GetJobTypeNames(bool reload)
        {
            return schedulerDefinitionService.GetJobTypeNames(reload);
        }

        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public Task<List<MisfireAction>> GetMisfireActions(TriggerType triggerType)
        {
            return schedulerDefinitionService.GetMisfireActions(triggerType);
        }

        [Authorize(QuartzAdminPermissions.Schedules.Default)]
        public Task<List<IntervalUnit>> GetTriggerIntervalUnits(TriggerType triggerType)
        {
            return schedulerDefinitionService.GetTriggerIntervalUnits(triggerType);
        }
    }
}

