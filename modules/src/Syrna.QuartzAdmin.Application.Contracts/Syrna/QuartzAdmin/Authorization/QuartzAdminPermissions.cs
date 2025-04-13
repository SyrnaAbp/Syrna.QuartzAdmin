using Volo.Abp.Reflection;

namespace Syrna.QuartzAdmin.Authorization
{
    public class QuartzAdminPermissions
    {
        public const string GroupName = "Syrna.QuartzAdmin";

        public class Schedules
        {
            public const string Default = GroupName + ".Schedules";

            public const string Create = Default + ".Create";

            public const string Update = Default + ".Update";

            public const string Delete = Default + ".Delete";
        }

        public class Overview
        {
            public const string Default = GroupName + ".Overview";
        }

        public class History
        {
            public const string Default = GroupName + ".History";

            public const string Delete = Default + ".Delete";
        }

        public static string[] GetAll()
        {
            return ReflectionHelper.GetPublicConstantsRecursively(typeof(QuartzAdminPermissions));
        }
    }
}