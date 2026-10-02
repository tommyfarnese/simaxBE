namespace SiMax.Api.Models;

public class TournamentCourt
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int CourtId { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public Court Court { get; set; } = null!;
}