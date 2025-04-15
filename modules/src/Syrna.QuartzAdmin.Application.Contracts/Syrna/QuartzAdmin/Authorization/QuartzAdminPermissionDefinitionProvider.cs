using Syrna.QuartzAdmin.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Syrna.QuartzAdmin.Authorization
{
    public class QuartzAdminPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var moduleGroup = context.AddGroup(QuartzAdminPermissions.GroupName, L("Permission:QuartzAdmin"));

            var schedulePermissions = moduleGroup.AddPermission(QuartzAdminPermissions.Schedules.Default, L("Permission:Schedules"));
            schedulePermissions.AddChild(QuartzAdminPermissions.Schedules.Create, L("Permission:Schedules.Create"));
            schedulePermissions.AddChild(QuartzAdminPermissions.Schedules.Update, L("Permission:Schedules.Update"));
            schedulePermissions.AddChild(QuartzAdminPermissions.Schedules.Delete, L("Permission:Schedules.Delete"));

            var triggersPermissions = moduleGroup.AddPermission(QuartzAdminPermissions.Triggers.Default, L("Permission:Triggers"));
            triggersPermissions.AddChild(QuartzAdminPermissions.Triggers.Create, L("Permission:Triggers.Create"));
            triggersPermissions.AddChild(QuartzAdminPermissions.Triggers.Update, L("Permission:Triggers.Update"));
            triggersPermissions.AddChild(QuartzAdminPermissions.Triggers.Delete, L("Permission:Triggers.Delete"));

            var historyPermissions = moduleGroup.AddPermission(QuartzAdminPermissions.History.Default, L("Permission:History"));
            historyPermissions.AddChild(QuartzAdminPermissions.History.Delete, L("Permission:History.Delete"));

            var overviewPermissions = moduleGroup.AddPermission(QuartzAdminPermissions.Overview.Default, L("Permission:Overview"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<QuartzAdminResource>(name);
        }
    }
}