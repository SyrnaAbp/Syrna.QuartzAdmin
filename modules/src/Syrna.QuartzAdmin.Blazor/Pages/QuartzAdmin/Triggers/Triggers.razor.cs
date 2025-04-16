using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Pages.QuartzAdmin.Triggers
{
    public partial class Triggers
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        [Inject] private ITriggersAppService TriggersAppService { get; set; } = null!;

        private int totalItems;
        private int pageSize = 10;
        private DataGrid<TriggerListItem> table = null!;
        private IEnumerable<TriggerListItem> pagedData;
        public async Task OnReadData()
        {
            var state = await table.GetState();
            pagedData = await TriggersAppService.GetAllTriggers();
            totalItems = pagedData.Count();
        }

        public Triggers()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //modalRef?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}