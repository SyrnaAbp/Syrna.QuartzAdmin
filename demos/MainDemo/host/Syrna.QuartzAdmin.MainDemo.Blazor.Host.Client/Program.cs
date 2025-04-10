using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.MainDemo.Blazor.Host.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        //builder.AddServiceDefaults();
        var application = await builder.AddApplicationAsync<MainDemoBlazorHostClientModule>(options =>
        {
            options.UseAutofac();
        });

        //builder.Services.AddBlazoredLocalStorage();
        //builder.Services.AddSingleton<IStorageService, BrowserStorageService>();
        var host = builder.Build();
        //if (builder.HostEnvironment.IsStaging())
        //{
        //    builder.WebHost.UseStaticWebAssets();
        //}
        await application.InitializeApplicationAsync(host.Services);
        await host.RunAsync();
    }
}
