using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPlayerNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerNumber",
                table: "Payment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerNumber",
                table: "Payment");
        }
    }
}
