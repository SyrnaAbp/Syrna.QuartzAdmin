using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Components.Web.Security;
using Volo.Abp.AspNetCore.Components.WebAssembly.LeptonXLiteTheme;
using Volo.Abp.UI.Navigation;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Host.Client.Components
{
    public partial class CustomUserMenu
    {
        [Inject]
        protected IOptions<AuthenticationOptions> AuthenticationOptions { get; set; }

        protected ApplicationMenu Menu { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Menu = await MenuManager.GetAsync(StandardMenus.User);

            Navigation.LocationChanged += OnLocationChanged;

            ApplicationConfigurationChangedService.Changed += ApplicationConfigurationChanged;
        }

        private async void ApplicationConfigurationChanged()
        {
            Menu = await MenuManager.GetAsync(StandardMenus.User);
            await InvokeAsync(StateHasChanged);
        }
    }
}