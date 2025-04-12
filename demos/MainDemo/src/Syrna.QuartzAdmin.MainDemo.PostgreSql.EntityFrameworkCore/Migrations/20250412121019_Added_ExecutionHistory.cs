using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "quartzadmin");

            migrationBuilder.CreateTable(
                name: "QuartzExecutionHistories",
                schema: "quartzadmin",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FireInstanceId = table.Column<string>(type: "text", nullable: true),
                    LogType = table.Column<string>(type: "text", nullable: false),
                    SchedulerInstanceId = table.Column<string>(type: "text", nullable: true),
                    SchedulerName = table.Column<string>(type: "text", nullable: true),
                    JobName = table.Column<string>(type: "text", nullable: true),
                    JobGroup = table.Column<string>(type: "text", nullable: true),
                    TriggerName = table.Column<string>(type: "text", nullable: true),
                    TriggerGroup = table.Column<string>(type: "text", nullable: true),
                    ScheduledFireTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FireTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Recovering = table.Column<bool>(type: "boolean", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    IsVetoed = table.Column<bool>(type: "boolean", nullable: true),
                    FinishedTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateAddedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: true),
                    IsException = table.Column<bool>(type: "boolean", nullable: true),
                    JobRunTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ReturnCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzExecutionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuartzJobSummaries",
                schema: "quartzadmin",
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

            migrationBuilder.CreateTable(
                name: "QuartzExecutionHistoryDetail",
                schema: "quartzadmin",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false),
                    ExecutionDetails = table.Column<string>(type: "text", nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "text", nullable: true),
                    ErrorCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorHelpLink = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzExecutionHistoryDetail", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_QuartzExecutionHistoryDetail_QuartzExecutionHistories_LogId",
                        column: x => x.LogId,
                        principalSchema: "quartzadmin",
                        principalTable: "QuartzExecutionHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuartzExecutionHistories_DateAddedUtc_LogType",
                schema: "quartzadmin",
                table: "QuartzExecutionHistories",
                columns: new[] { "DateAddedUtc", "LogType" });

            migrationBuilder.CreateIndex(
                name: "IX_QuartzExecutionHistories_FireInstanceId",
                schema: "quartzadmin",
                table: "QuartzExecutionHistories",
                column: "FireInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuartzExecutionHistories_TriggerName_TriggerGroup_JobName_J~",
                schema: "quartzadmin",
                table: "QuartzExecutionHistories",
                columns: new[] { "TriggerName", "TriggerGroup", "JobName", "JobGroup", "DateAddedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuartzExecutionHistoryDetail",
                schema: "quartzadmin");

            migrationBuilder.DropTable(
                name: "QuartzJobSummaries",
                schema: "quartzadmin");

            migrationBuilder.DropTable(
                name: "QuartzExecutionHistories",
                schema: "quartzadmin");
        }
    }
}
