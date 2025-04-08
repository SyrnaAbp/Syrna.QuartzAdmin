using JetBrains.Annotations;
using Volo.Abp.MongoDB;

namespace Syrna.QuartzAdmin.MongoDB
{
    public class QuartzAdminMongoModelBuilderConfigurationOptions : AbpMongoModelBuilderConfigurationOptions
    {
        public QuartzAdminMongoModelBuilderConfigurationOptions(
            [NotNull] string collectionPrefix = "")
            : base(collectionPrefix)
        {
        }
    }
}