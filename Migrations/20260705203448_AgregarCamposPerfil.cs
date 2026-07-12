using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StravaTeamApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StravaAthleteId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UPIN",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genero",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UPIN",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<long>(
                name: "StravaAthleteId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);
        }
    }
}
