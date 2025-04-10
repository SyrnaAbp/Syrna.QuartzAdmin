namespace Syrna.QuartzAdmin
{
    public static class QuartzAdminDbProperties
    {
        public static string DbTablePrefix { get; set; } = "Quartz";

        public static string DbSchema { get; set; } = "QuartzAdmin";

        public const string ConnectionStringName = "SyrnaQuartzAdmin";
    }
}
