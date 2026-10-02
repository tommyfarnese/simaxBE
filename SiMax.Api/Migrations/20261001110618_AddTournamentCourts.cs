using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentCourts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentCourts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    CourtId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentCourts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentCourts_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentCourts_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentCourts_CourtId",
                table: "TournamentCourts",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentCourts_TournamentId_CourtId",
                table: "TournamentCourts",
                columns: new[] { "TournamentId", "CourtId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentCourts");
        }
    }
}
