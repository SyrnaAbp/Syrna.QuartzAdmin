using System;
using Volo.Abp.ObjectExtending.Modularity;

namespace Syrna.QuartzAdmin.ObjectExtending;

public class QuartzAdminModuleExtensionConfiguration : ModuleExtensionConfiguration
{
    public QuartzAdminModuleExtensionConfiguration ConfigurePrivateMessage(
        Action<EntityExtensionConfiguration> configureAction)
    {
        return this.ConfigureEntity(
            QuartzAdminModuleExtensionConsts.EntityNames.PrivateMessage,
            configureAction
        );
    }
}
