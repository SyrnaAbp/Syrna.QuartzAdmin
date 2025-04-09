using Microsoft.AspNetCore.Mvc;
using Syrna.QuartzAdmin.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Syrna.QuartzAdmin
{
    [Area(QuartzAdminRemoteServiceConsts.ModuleName)]
    public abstract class QuartzAdminController : AbpController
    {
        protected QuartzAdminController()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }
    }
}
