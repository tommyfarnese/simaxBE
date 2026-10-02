namespace SiMax.Api.DTOs.Admin;

public class MatchDto
{
    public int Id { get; set; }

    public int MatchNumber { get; set; }

    public int PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;

    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int Team1RegistrationId { get; set; }
    public string Team1Name { get; set; } = string.Empty;

    public int Team2RegistrationId { get; set; }
    public string Team2Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? Team1Score { get; set; }
    public int? Team2Score { get; set; }
}