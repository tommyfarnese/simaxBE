namespace SiMax.Api.Services.Scheduling;

public class MatchSchedulingResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public List<ScheduledMatch> Matches { get; set; } = new();
}