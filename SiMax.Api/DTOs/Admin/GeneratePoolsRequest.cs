using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class GeneratePoolsRequest
{
    [Range(1, 20)]
    public int PoolCount { get; set; }

    [Range(1, 20)]
    public int MaxTeamsPerPool { get; set; }

    [Required]
    public string Criterion { get; set; } = string.Empty;
}