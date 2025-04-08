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
            
            var privateMessages = moduleGroup.AddPermission(QuartzAdminPermissions.PrivateMessages.Default, L("Permission:PrivateMessage"));
            privateMessages.AddChild(QuartzAdminPermissions.PrivateMessages.Create, L("Permission:Create"));
            privateMessages.AddChild(QuartzAdminPermissions.PrivateMessages.SetRead, L("Permission:SetRead"));
            privateMessages.AddChild(QuartzAdminPermissions.PrivateMessages.Delete, L("Permission:Delete"));
            
            var privateMessageNotifications = moduleGroup.AddPermission(QuartzAdminPermissions.PrivateMessageNotifications.Default, L("Permission:PrivateMessageNotification"));
            privateMessageNotifications.AddChild(QuartzAdminPermissions.PrivateMessageNotifications.Delete, L("Permission:Delete"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<QuartzAdminResource>(name);
        }
    }
}