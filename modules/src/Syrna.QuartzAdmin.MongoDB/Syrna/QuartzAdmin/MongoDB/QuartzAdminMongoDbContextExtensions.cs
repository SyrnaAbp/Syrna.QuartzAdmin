using System;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace Syrna.QuartzAdmin.MongoDB
{
    public static class QuartzAdminMongoDbContextExtensions
    {
        public static void ConfigureQuartzAdmin(
            this IMongoModelBuilder builder,
            Action<AbpMongoModelBuilderConfigurationOptions> optionsAction = null)
        {
            Check.NotNull(builder, nameof(builder));

            var options = new QuartzAdminMongoModelBuilderConfigurationOptions(
                QuartzAdminDbProperties.DbTablePrefix
            );

            optionsAction?.Invoke(options);
        }
    }
}