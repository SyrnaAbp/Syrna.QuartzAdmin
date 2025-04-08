using System.Threading.Tasks;
using Syrna.QuartzAdmin.Authorization;
using Syrna.QuartzAdmin.Localization;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.Web.Menus;

public class QuartzAdminMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<QuartzAdminResource>();
        //Add main menu items.

        if (await context.IsGrantedAsync(QuartzAdminPermissions.PrivateMessages.Default))
        {
            context.Menu.GetAdministration().AddItem(new ApplicationMenuItem(QuartzAdminMenus.Prefix,
                displayName: l["Menu:PrivateMessage"], "~/QuartzAdmin/PrivateMessages/PrivateMessage", icon: "fa fa-messages"));
        }
    }
}