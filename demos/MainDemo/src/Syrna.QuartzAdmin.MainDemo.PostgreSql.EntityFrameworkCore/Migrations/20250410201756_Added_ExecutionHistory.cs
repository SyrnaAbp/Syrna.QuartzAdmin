using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syrna.QuartzAdmin.MainDemo.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Added_ExecutionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "QuartzAdmin");

            migrationBuilder.CreateTable(
                name: "QuartzExecutionHistories",
                schema: "QuartzAdmin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FireInstanceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SchedulerInstanceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SchedulerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Job = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Trigger = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ScheduledFireTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualFireTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Recovering = table.Column<bool>(type: "boolean", nullable: false),
                    Vetoed = table.Column<bool>(type: "boolean", nullable: false),
                    FinishedTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzExecutionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuartzJobSummaries",
                schema: "QuartzAdmin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchedulerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TotalJobsExecuted = table.Column<int>(type: "integer", nullable: false),
                    TotalJobsFailed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzJobSummaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuartzExecutionHistories_FireInstanceId",
                schema: "QuartzAdmin",
                table: "QuartzExecutionHistories",
                column: "FireInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuartzExecutionHistories",
                schema: "QuartzAdmin");

            migrationBuilder.DropTable(
                name: "QuartzJobSummaries",
                schema: "QuartzAdmin");
        }
    }
}
