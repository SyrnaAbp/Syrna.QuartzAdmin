using Blazorise;
using Microsoft.Extensions.Configuration;
using Syrna.QuartzAdmin.Authorization;
using Syrna.QuartzAdmin.MainDemo.Localization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Host.Client.Menus;

public class MainDemoBlazorHostClientMenuContributor(IConfiguration configuration) : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<MainDemoResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                MainDemoBlazorHostClientMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fa-home"
            )
        );

        var groupMenuItem = new ApplicationMenuItem(MainDemoBlazorHostClientMenus.Prefix, l["Menu:QuartzAdmin"], icon: IconName.Clock.ToString());
        context.Menu.AddItem(groupMenuItem);

        groupMenuItem.AddItem(new ApplicationMenuItem(
            MainDemoBlazorHostClientMenus.Overview,
            l["Menu:Overview"],
            url: "~/overview").RequirePermissions(QuartzAdminPermissions.Overview.Default));

        groupMenuItem.AddItem(new ApplicationMenuItem(
            MainDemoBlazorHostClientMenus.Schedules,
            l["Menu:Schedules"],
            url: "~/schedules").RequirePermissions(QuartzAdminPermissions.Schedules.Default));

        groupMenuItem.AddItem(new ApplicationMenuItem(
            MainDemoBlazorHostClientMenus.History,
            l["Menu:History"],
            url: "~/history").RequirePermissions(QuartzAdminPermissions.History.Default));

        return Task.CompletedTask;
    }

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var accountStringLocalizer = context.GetLocalizer<MainDemoResource>();

        var identityServerUrl = configuration["AuthServer:Authority"] ?? "";

        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Manage",
            accountStringLocalizer["MyAccount"],
            $"{identityServerUrl.EnsureEndsWith('/')}Account/Manage?returnUrl={configuration["App:SelfUrl"]}",
            icon: "fa fa-cog",
            order: 1000,
            null).RequireAuthenticated());

        return Task.CompletedTask;
    }
}
