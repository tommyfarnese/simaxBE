namespace SiMax.Api.Models;

public class FinalMatch
{
    public int Id { get; set; }

    public int FinalPhaseId { get; set; }

    public int RoundNumber { get; set; }

    public int MatchNumber { get; set; }

    public int? CourtId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? Team1RegistrationId { get; set; }

    public int? Team2RegistrationId { get; set; }

    public int? Team1Score { get; set; }

    public int? Team2Score { get; set; }

    public string Status { get; set; } = "Scheduled";

    // Partita precedente dalla quale arriverà Team 1
    public int? Team1SourceMatchId { get; set; }

    // Partita precedente dalla quale arriverà Team 2
    public int? Team2SourceMatchId { get; set; }

    public FinalPhase FinalPhase { get; set; } = null!;

    public Court? Court { get; set; }

    public Registration? Team1 { get; set; }

    public Registration? Team2 { get; set; }

    public FinalMatch? Team1SourceMatch { get; set; }

    public FinalMatch? Team2SourceMatch { get; set; }
}