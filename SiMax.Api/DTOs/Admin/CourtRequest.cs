using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class CourtRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}