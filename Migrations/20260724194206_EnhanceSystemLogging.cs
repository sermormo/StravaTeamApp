using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StravaTeamApp.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceSystemLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorUserId",
                table: "SystemLogs",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SystemLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "SystemLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "LegacyEvent");

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "SystemLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestPath",
                table: "SystemLogs",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Category",
                table: "SystemLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Fecha",
                table: "SystemLogs",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Nivel",
                table: "SystemLogs",
                column: "Nivel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_Category",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_Fecha",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_Nivel",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "RequestPath",
                table: "SystemLogs");
        }
    }
}
