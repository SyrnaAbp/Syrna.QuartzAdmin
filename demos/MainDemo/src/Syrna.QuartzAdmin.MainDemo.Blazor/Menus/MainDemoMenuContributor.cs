using Syrna.Abp.DynamicMenu.Demo.Blazor.Menus;
using Syrna.QuartzAdmin.MainDemo.Localization;
using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Menus
{
    public class MainDemoMenuContributor : IMenuContributor
    {
        public virtual Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            if (context.Menu.Name != StandardMenus.Main)
            {
                return Task.CompletedTask;
            }

            var administrationMenu = context.Menu.GetAdministration();
            administrationMenu.Icon = "fa fa-gears";

            var l = context.GetLocalizer<MainDemoResource>();

            var identityMenuItem = new ApplicationMenuItem(MainDemoMenus.Prefix, l["Menu:MainDemo"],
                icon: "fa fa-user");
            administrationMenu.AddItem(identityMenuItem);

            return Task.CompletedTask;
        }
    }
}