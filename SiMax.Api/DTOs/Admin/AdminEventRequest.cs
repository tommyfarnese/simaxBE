using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class AdminEventRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PeriodLabel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [Range(0, 10000)]
    public int PriceOneTournament { get; set; }

    [Range(0, 10000)]
    public int PriceTwoTournaments { get; set; }

    [Range(0, 10000)]
    public int PriceThreeTournaments { get; set; }
}