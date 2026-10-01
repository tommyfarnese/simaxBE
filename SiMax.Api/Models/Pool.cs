namespace SiMax.Api.Models;

public class Pool
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public ICollection<PoolEntry> Entries { get; set; } = new List<PoolEntry>();
}