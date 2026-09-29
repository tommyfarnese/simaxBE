namespace SiMax.Api.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}