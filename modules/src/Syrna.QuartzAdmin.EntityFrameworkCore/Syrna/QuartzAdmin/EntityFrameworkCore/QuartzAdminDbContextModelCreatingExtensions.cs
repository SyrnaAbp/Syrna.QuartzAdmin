using Microsoft.EntityFrameworkCore;
using System;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Syrna.QuartzAdmin.EntityFrameworkCore
{
    public static class QuartzAdminDbContextModelCreatingExtensions
    {
        public static void ConfigureQuartzAdmin(
            this ModelBuilder builder,
            Action<QuartzAdminModelBuilderConfigurationOptions> optionsAction = null)
        {
            Check.NotNull(builder, nameof(builder));

            var options = new QuartzAdminModelBuilderConfigurationOptions(
                QuartzAdminDbProperties.DbTablePrefix,
                QuartzAdminDbProperties.DbSchema
            );

            optionsAction?.Invoke(options);

        }
    }
}
