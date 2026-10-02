using System.Text.RegularExpressions;

namespace SiMax.Api.Models;

public class Court
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public Event Event { get; set; } = null!;

    public ICollection<CourtAvailability> Availabilities { get; set; }
        = new List<CourtAvailability>();

    public ICollection<Match> Matches { get; set; }
        = new List<Match>();

    public ICollection<TournamentCourt> TournamentCourts { get; set; }
    = new List<TournamentCourt>();
}