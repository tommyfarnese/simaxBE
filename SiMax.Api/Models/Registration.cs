using System.ComponentModel.DataAnnotations.Schema;

namespace SiMax.Api.Models;

public class Registration
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string TeamName { get; set; } = string.Empty;

    public string Player1FirstName { get; set; } = string.Empty;

    public string Player1LastName { get; set; } = string.Empty;

    public string Player1Phone { get; set; } = string.Empty;

    public string Player2FirstName { get; set; } = string.Empty;

    public string Player2LastName { get; set; } = string.Empty;

    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(TournamentId))]
    public Tournament? Tournament { get; set; }
}