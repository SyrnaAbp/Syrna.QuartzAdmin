using Syrna.QuartzAdmin.Localization;
using Volo.Abp.Application.Services;

namespace Syrna.QuartzAdmin
{
    public abstract class QuartzAdminAppService : ApplicationService
    {
        protected QuartzAdminAppService()
        {
            LocalizationResource = typeof(QuartzAdminResource);
            ObjectMapperContext = typeof(QuartzAdminApplicationModule);
        }
    }
}
