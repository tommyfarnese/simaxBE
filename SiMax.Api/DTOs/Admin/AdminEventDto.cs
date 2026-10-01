namespace SiMax.Api.DTOs.Admin;

public class AdminEventDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PeriodLabel { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int PriceOneTournament { get; set; }
    public int PriceTwoTournaments { get; set; }
    public int PriceThreeTournaments { get; set; }
}