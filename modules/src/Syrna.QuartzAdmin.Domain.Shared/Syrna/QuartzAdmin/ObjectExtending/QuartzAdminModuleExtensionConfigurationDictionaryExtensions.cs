using System;
using Volo.Abp.ObjectExtending.Modularity;

namespace Syrna.QuartzAdmin.ObjectExtending;

public static class QuartzAdminModuleExtensionConfigurationDictionaryExtensions
{
    public static ModuleExtensionConfigurationDictionary ConfigureQuartzAdmin(
        this ModuleExtensionConfigurationDictionary modules,
        Action<QuartzAdminModuleExtensionConfiguration> configureAction)
    {
        return modules.ConfigureModule(
            QuartzAdminModuleExtensionConsts.ModuleName,
            configureAction
        );
    }
}
