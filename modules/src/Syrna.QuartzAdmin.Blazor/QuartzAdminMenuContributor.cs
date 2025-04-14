using Blazorise;
using Syrna.Alpha.AccountingDef.Blazor;
using Syrna.QuartzAdmin.Authorization;
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
       
        var groupMenuItem = new ApplicationMenuItem(QuartzAdminMenuNames.GroupName, l["Menu:QuartzAdmin"], icon: "fa fa-clock");
        context.Menu.AddItem(groupMenuItem);

        groupMenuItem.AddItem(new ApplicationMenuItem(
            QuartzAdminMenuNames.Overview,
            l["Menu:Overview"],
            icon: "fa fa-magnifying-glass-chart",
            url: "~/QuartzAdmin/Overview").RequirePermissions(QuartzAdminPermissions.Overview.Default));

        groupMenuItem.AddItem(new ApplicationMenuItem(
            QuartzAdminMenuNames.Schedules,
            l["Menu:Schedules"],
            icon:"fa fa-bell",
            url: "~/QuartzAdmin/Schedules").RequirePermissions(QuartzAdminPermissions.Schedules.Default));

        groupMenuItem.AddItem(new ApplicationMenuItem(
            QuartzAdminMenuNames.History,
            l["Menu:History"],
            icon:"fa fa-timeline",
            url: "~/QuartzAdmin/History").RequirePermissions(QuartzAdminPermissions.History.Default));

        return Task.CompletedTask;
    }
}
