namespace SiMax.Api.DTOs;

public class TournamentDto
{
    public int Id { get; set; }

    public int SortOrder { get; set; }

    public string TabLabel { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public decimal Price { get; set; }

    public int MaxTeams { get; set; }

    public string Level { get; set; } = string.Empty;

    public int MinPlayers { get; set; }

    public string FormUrl { get; set; } = string.Empty;

    public string WaitlistFormUrl { get; set; } = string.Empty;

    public CategoryDto Category { get; set; } = null!;
}