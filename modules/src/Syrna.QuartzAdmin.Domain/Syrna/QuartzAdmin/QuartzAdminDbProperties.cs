namespace Syrna.QuartzAdmin
{
    public static class QuartzAdminDbProperties
    {
        public static string DbTablePrefix { get; set; } = "Quartz";

        public static string DbSchema { get; set; } = "quartzadmin";

        public const string ConnectionStringName = "SyrnaQuartzAdmin";
    }
}
