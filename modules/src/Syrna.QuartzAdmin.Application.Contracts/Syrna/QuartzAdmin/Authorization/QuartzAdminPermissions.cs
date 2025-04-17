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
            public const string ShutDown = Default + ".ShutDown";
            public const string Standby = Default + ".Standby";
            public const string Trigger = Default + ".Trigger";
            public const string Interrupt = Default + ".Interrupt";
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

        public class Triggers
        {
            public const string Default = GroupName + ".Triggers";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        public class Jobs
        {
            public const string Default = GroupName + ".Jobs";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Trigger = Default + ".Trigger";
            public const string Interrupt = Default + ".Interrupt";
        }

        public static string[] GetAll()
        {
            return ReflectionHelper.GetPublicConstantsRecursively(typeof(QuartzAdminPermissions));
        }
    }
}