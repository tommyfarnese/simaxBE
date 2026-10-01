namespace SiMax.Api.Models;

public class PoolEntry
{
    public int Id { get; set; }

    public int PoolId { get; set; }

    public int RegistrationId { get; set; }

    public int Position { get; set; }

    public Pool Pool { get; set; } = null!;

    public Registration Registration { get; set; } = null!;
}