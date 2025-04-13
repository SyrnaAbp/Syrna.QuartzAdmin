using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.Scheduler
{
    public interface ISchedulerDefinitionService : IApplicationService
    {
        Task<List<IntervalUnit>> GetTriggerIntervalUnits(TriggerType triggerType);
        Task<List<MisfireAction>> GetMisfireActions(TriggerType triggerType);
        /// <summary>
        /// Return available IJob implementations
        /// </summary>
        /// <param name="reload"></param>
        /// <returns></returns>
        Task<List<string>> GetJobTypeNames(bool reload);
    }
}

