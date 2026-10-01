namespace SiMax.Api.DTOs.Admin;

public class TournamentStandingsDto
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public List<StandingDto> Standings { get; set; } = new();
}