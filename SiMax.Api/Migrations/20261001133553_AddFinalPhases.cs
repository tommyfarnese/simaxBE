using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalPhases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalPhases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EliminationType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalPhases_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalPhaseQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalPhaseId = table.Column<int>(type: "int", nullable: false),
                    PoolPosition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalPhaseQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalPhaseQualifications_FinalPhases_FinalPhaseId",
                        column: x => x.FinalPhaseId,
                        principalTable: "FinalPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalPhaseQualifications_FinalPhaseId_PoolPosition",
                table: "FinalPhaseQualifications",
                columns: new[] { "FinalPhaseId", "PoolPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalPhases_TournamentId",
                table: "FinalPhases",
                column: "TournamentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinalPhaseQualifications");

            migrationBuilder.DropTable(
                name: "FinalPhases");
        }
    }
}
