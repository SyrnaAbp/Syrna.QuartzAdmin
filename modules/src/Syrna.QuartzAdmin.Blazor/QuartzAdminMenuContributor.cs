using Blazorise;
using Syrna.Alpha.AccountingDef.Blazor;
using Syrna.QuartzAdmin.Localization;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.Blazor;

public class QuartzAdminMenuContributor : IMenuContributor
{
    public virtual Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return Task.CompletedTask;
        }

        var l = context.GetLocalizer<QuartzAdminResource>();
       
        var groupMenuItem = new ApplicationMenuItem(QuartzAdminMenuNames.GroupName, l["Menu:QuartzAdmin"], icon: IconName.Clock.ToString());
        context.Menu.AddItem(groupMenuItem);

        groupMenuItem.AddItem(new ApplicationMenuItem(
            QuartzAdminMenuNames.Overview,
            l["Menu:Overview"],
            url: "~/overview")/*.RequirePermissions(QuartzAdminPermissions.Overview.Default)*/);

        groupMenuItem.AddItem(new ApplicationMenuItem(
            QuartzAdminMenuNames.Schedules,
            l["Menu:Schedules"],
            url: "~/schedules")/*.RequirePermissions(QuartzAdminPermissions.Schedules.Default)*/);

        groupMenuItem.AddItem(new ApplicationMenuItem(
            QuartzAdminMenuNames.History,
            l["Menu:History"],
            url: "~/history")/*.RequirePermissions(QuartzAdminPermissions.History.Default)*/);

        return Task.CompletedTask;
    }
}
