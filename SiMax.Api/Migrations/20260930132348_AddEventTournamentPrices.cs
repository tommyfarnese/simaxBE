using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTournamentPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceOneTournament",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PriceThreeTournaments",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PriceTwoTournaments",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PriceOneTournament", "PriceThreeTournaments", "PriceTwoTournaments" },
                values: new object[] { 0, 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceOneTournament",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PriceThreeTournaments",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PriceTwoTournaments",
                table: "Events");
        }
    }
}
