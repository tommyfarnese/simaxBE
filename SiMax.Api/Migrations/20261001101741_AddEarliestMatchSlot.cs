using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEarliestMatchSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EarliestMatchSlot",
                table: "Registrations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarliestMatchSlot",
                table: "Registrations");
        }
    }
}
