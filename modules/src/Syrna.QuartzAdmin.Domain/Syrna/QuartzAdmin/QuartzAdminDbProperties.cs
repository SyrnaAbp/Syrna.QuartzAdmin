namespace Syrna.QuartzAdmin
{
    public static class QuartzAdminDbProperties
    {
        public static string DbTablePrefix { get; set; } = "Pm";

        public static string DbSchema { get; set; } = null;

        public const string ConnectionStringName = "SyrnaQuartzAdmin";
    }
}
