using Syrna.QuartzAdmin.MainDemo.Localization;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.MainDemo;

/* Inherit your application services from this class.
 */
public abstract class MainDemoAppService : ApplicationService
{
    protected MainDemoAppService()
    {
        LocalizationResource = typeof(MainDemoResource);
    }
}
