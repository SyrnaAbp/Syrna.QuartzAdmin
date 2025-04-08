using JetBrains.Annotations;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Syrna.QuartzAdmin.EntityFrameworkCore
{
    public class QuartzAdminModelBuilderConfigurationOptions : AbpModelBuilderConfigurationOptions
    {
        public QuartzAdminModelBuilderConfigurationOptions(
            [NotNull] string tablePrefix = "",
            [CanBeNull] string schema = null)
            : base(
                tablePrefix,
                schema)
        {

        }
    }
}