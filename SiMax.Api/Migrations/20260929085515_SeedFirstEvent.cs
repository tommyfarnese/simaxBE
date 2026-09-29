using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SiMax.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedFirstEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Address", "Date", "Location", "PeriodLabel", "Title" },
                values: new object[] { 1, "Largo Antonio Balestra 5, Milano", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "PalaUno", "Mattina & Pomeriggio", "Torneo di Fine Estate" });

            migrationBuilder.InsertData(
                table: "Tournaments",
                columns: new[] { "Id", "CategoryId", "EndTime", "EventId", "FormUrl", "Format", "Level", "MaxTeams", "MinPlayers", "Price", "SortOrder", "StartTime", "TabLabel", "Title", "WaitlistFormUrl" },
                values: new object[,]
                {
                    { 1, 1, new TimeSpan(0, 14, 0, 0, 0), 1, "https://docs.google.com/forms/d/e/1FAIpQLSceRph3nNrHEA_c-SrICg_OuU23-RkOtSAd2ft9s7TXhpOqkQ/viewform?usp=header", "2×2", "Open", 10, 2, 25, 1, new TimeSpan(0, 9, 30, 0, 0), "2×2 MM", "Torneo di Fine Estate MM", "" },
                    { 2, 2, new TimeSpan(0, 14, 0, 0, 0), 1, "https://docs.google.com/forms/d/e/1FAIpQLSdQvcEcQxF_3ouLmuEWYggWa9Otg2L8MD92jHy7DwKaU1clcA/viewform?usp=header", "2×2", "Open", 10, 2, 25, 2, new TimeSpan(0, 9, 30, 0, 0), "2×2 FF", "Torneo di Fine Estate FF", "" },
                    { 3, 3, new TimeSpan(0, 18, 30, 0, 0), 1, "https://docs.google.com/forms/d/e/1FAIpQLScNsRCRIIgpqGM1kE3xl3ZjnHVu6-BF0fViXYKSO8scBSaGcQ/viewform?usp=header", "2×2", "Open", 16, 2, 25, 3, new TimeSpan(0, 14, 0, 0, 0), "2×2 MIX", "Torneo di Fine Estate MIX", "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
