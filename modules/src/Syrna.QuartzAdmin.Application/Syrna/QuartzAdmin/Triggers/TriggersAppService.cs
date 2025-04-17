using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Syrna.QuartzAdmin.Authorization;
using Syrna.QuartzAdmin.Jobs;
using Volo.Abp;

namespace Syrna.QuartzAdmin.Triggers
{
    public class TriggersAppService : QuartzAdminAppService, ITriggersAppService
    {
        private IScheduler Scheduler => LazyServiceProvider.LazyGetRequiredService<IScheduler>();

        /// <summary>
        /// Getting a list of <see cref="JobListDetail"/> for all configured <see cref="IJob"/> instances in the <see cref="IScheduler"/>.
        /// </summary>
        /// <returns>The list of configured triggers.</returns>
        /// <response code="200">Returns the list of configured triggers for the scheduler..</response>
        /// <response code="500">Returns the internal server error..</response>
        [HttpGet]
        [Authorize(QuartzAdminPermissions.Triggers.Default)]
        public async Task<List<TriggerListItem>> GetAllTriggers()
        {
            try
            {
                var keys = (await Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup())).OrderBy(x => x.ToString());
                var list = new List<TriggerListItem>();

                foreach (var key in keys)
                {
                    var t = await GetTrigger(key, Scheduler);
                    var state = await Scheduler.GetTriggerState(key);

                    list.Add(new TriggerListItem()
                    {
                        Type = t.GetTriggerType(),
                        TriggerName = t.Key.Name,
                        TriggerGroup = t.Key.Group,
                        IsPaused = state == TriggerState.Paused,
                        ScheduleDescription = t.GetScheduleDescription(),
                        StartTimeUtc = t.StartTimeUtc,
                        EndTimeUtc = t.FinalFireTimeUtc,
                        LastFireTimeUtc = t.GetPreviousFireTimeUtc(),
                        NextFireTimeUtc = t.GetNextFireTimeUtc(),
                        ClrType = t.GetType().Name,
                        Description = t.Description,
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UserFriendlyException("Can not get all triggers", "CantGetAllTriggers", innerException: ex);
            }
        }


        [Authorize(QuartzAdminPermissions.Triggers.Default)]
        private static async Task<ITrigger> GetTrigger(TriggerKey key, IScheduler scheduler)
        {
            var trigger = await scheduler.GetTrigger(key);

            if (trigger == null)
            {
                throw new InvalidOperationException("Trigger " + key + " not found.");
            }

            return trigger;
        }
    }
}
