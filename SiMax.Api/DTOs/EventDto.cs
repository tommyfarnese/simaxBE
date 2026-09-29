namespace SiMax.Api.DTOs;

public class EventDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PeriodLabel { get; set; } = string.Empty;

    public List<TournamentDto> Tournaments { get; set; } = new();
}