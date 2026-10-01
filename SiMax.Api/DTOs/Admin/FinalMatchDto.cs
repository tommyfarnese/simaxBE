namespace SiMax.Api.DTOs.Admin;

public class FinalMatchDto
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

    public string Status { get; set; } = string.Empty;

    public int? Team1SourceMatchId { get; set; }

    public int? Team2SourceMatchId { get; set; }
}