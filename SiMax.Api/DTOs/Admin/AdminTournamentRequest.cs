using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class AdminTournamentRequest
{
    [Required]
    public int EventId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int SortOrder { get; set; }

    [Required]
    [MaxLength(50)]
    public string TabLabel { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Format { get; set; } = string.Empty;

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Range(0, 10000)]
    public int Price { get; set; }

    [Range(1, 1000)]
    public int MaxTeams { get; set; }

    [MaxLength(100)]
    public string Level { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MinPlayers { get; set; }

    [MaxLength(1000)]
    public string FormUrl { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string WaitlistFormUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string GoogleFormId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}