using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalPhaseId = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    MatchNumber = table.Column<int>(type: "int", nullable: false),
                    CourtId = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Team1RegistrationId = table.Column<int>(type: "int", nullable: true),
                    Team2RegistrationId = table.Column<int>(type: "int", nullable: true),
                    Team1Score = table.Column<int>(type: "int", nullable: true),
                    Team2Score = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team1SourceMatchId = table.Column<int>(type: "int", nullable: true),
                    Team2SourceMatchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalMatches_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinalMatches_FinalMatches_Team1SourceMatchId",
                        column: x => x.Team1SourceMatchId,
                        principalTable: "FinalMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinalMatches_FinalMatches_Team2SourceMatchId",
                        column: x => x.Team2SourceMatchId,
                        principalTable: "FinalMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinalMatches_FinalPhases_FinalPhaseId",
                        column: x => x.FinalPhaseId,
                        principalTable: "FinalPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalMatches_Registrations_Team1RegistrationId",
                        column: x => x.Team1RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinalMatches_Registrations_Team2RegistrationId",
                        column: x => x.Team2RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalMatches_CourtId",
                table: "FinalMatches",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalMatches_FinalPhaseId",
                table: "FinalMatches",
                column: "FinalPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalMatches_Team1RegistrationId",
                table: "FinalMatches",
                column: "Team1RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalMatches_Team1SourceMatchId",
                table: "FinalMatches",
                column: "Team1SourceMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalMatches_Team2RegistrationId",
                table: "FinalMatches",
                column: "Team2RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalMatches_Team2SourceMatchId",
                table: "FinalMatches",
                column: "Team2SourceMatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinalMatches");
        }
    }
}
