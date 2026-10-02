namespace SiMax.Api.Models;

public class Match
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int PoolId { get; set; }

    public int CourtId { get; set; }

    public int MatchNumber { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Team1RegistrationId { get; set; }

    public int Team2RegistrationId { get; set; }

    public string Status { get; set; } = "Scheduled";

    public Tournament Tournament { get; set; } = null!;

    public Pool Pool { get; set; } = null!;

    public Court Court { get; set; } = null!;

    public Registration Team1 { get; set; } = null!;

    public Registration Team2 { get; set; } = null!;

    public int? Team1Score { get; set; }

    public int? Team2Score { get; set; }
}