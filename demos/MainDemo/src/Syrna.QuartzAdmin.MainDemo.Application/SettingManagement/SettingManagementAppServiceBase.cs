using Syrna.QuartzAdmin.MainDemo;
using Syrna.QuartzAdmin.MainDemo.Localization;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin.MainDemo.SettingManagement;

public abstract class SettingManagementAppServiceBase : ApplicationService
{
    protected SettingManagementAppServiceBase()
    {
        ObjectMapperContext = typeof(MainDemoApplicationModule);
        LocalizationResource = typeof(MainDemoResource);
    }
}
