using System.Threading.Tasks;
using Syrna.QuartzAdmin.Authorization;
using Volo.Abp.AspNetCore.Components.Web.Theming.Toolbars;

namespace Syrna.QuartzAdmin.Blazor
{
    public class QuartzAdminToolbarContributor : IToolbarContributor
    {
        public virtual async Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
        {
            if (context.Toolbar.Name != StandardToolbars.Main)
            {
                return;
            }

            if (await context.IsGrantedAsync(QuartzAdminPermissions.Schedules.Default))
            {
                //context.Toolbar.Items.Insert(0, new ToolbarItem(typeof(PmNotificationViewComponent)));
            }
        }
    }
}