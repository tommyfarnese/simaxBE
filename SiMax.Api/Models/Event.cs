namespace SiMax.Api.Models;

public class Event
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PeriodLabel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}