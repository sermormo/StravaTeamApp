using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StravaTeamApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalElevationGain",
                table: "Activities",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalElevationGain",
                table: "Activities");
        }
    }
}
