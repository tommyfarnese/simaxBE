using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinalPhaseQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinalPhaseQualifications_FinalPhaseId_PoolPosition",
                table: "FinalPhaseQualifications");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "FinalPhaseQualifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RuleType",
                table: "FinalPhaseQualifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FinalPhaseQualifications_FinalPhaseId",
                table: "FinalPhaseQualifications",
                column: "FinalPhaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinalPhaseQualifications_FinalPhaseId",
                table: "FinalPhaseQualifications");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "FinalPhaseQualifications");

            migrationBuilder.DropColumn(
                name: "RuleType",
                table: "FinalPhaseQualifications");

            migrationBuilder.CreateIndex(
                name: "IX_FinalPhaseQualifications_FinalPhaseId_PoolPosition",
                table: "FinalPhaseQualifications",
                columns: new[] { "FinalPhaseId", "PoolPosition" },
                unique: true);
        }
    }
}
