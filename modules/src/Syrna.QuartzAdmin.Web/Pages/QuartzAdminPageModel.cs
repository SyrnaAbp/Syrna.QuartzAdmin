using Syrna.QuartzAdmin.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Syrna.QuartzAdmin.Web.Pages
{
    /* Inherit your PageModel classes from this class.
     */
    public abstract class QuartzAdminPageModel : AbpPageModel
    {
        protected QuartzAdminPageModel()
        {
            LocalizationResourceType = typeof(QuartzAdminResource);
            ObjectMapperContext = typeof(QuartzAdminWebModule);
        }
    }
}