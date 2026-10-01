namespace SiMax.Api.DTOs.Admin;

public class StandingDto
{
    public int Position { get; set; }

    public int RegistrationId { get; set; }
    public string TeamName { get; set; } = string.Empty;

    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }

    public int PointsFor { get; set; }
    public int PointsAgainst { get; set; }
    public int PointDifference { get; set; }
}