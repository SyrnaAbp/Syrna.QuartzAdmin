using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.Triggers
{
    public interface ITriggersAppService : IApplicationService
    {
        Task<List<TriggerListItem>> GetAllTriggers();
    }
}