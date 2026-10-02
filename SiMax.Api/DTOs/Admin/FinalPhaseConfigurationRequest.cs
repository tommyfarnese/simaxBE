using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class FinalPhaseConfigurationRequest
{
    [Required]
    public List<FinalPhaseRequest> Phases { get; set; } = new();
}

public class FinalPhaseRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public List<FinalPhaseQualificationRequest> Rules { get; set; } = new();

    [Required]
    public string EliminationType { get; set; } = "SingleElimination";
}

public class FinalPhaseQualificationRequest
{
    [Required]
    public string RuleType { get; set; } = "Position";

    [Range(1, 100)]
    public int PoolPosition { get; set; }

    [Range(1, 100)]
    public int Count { get; set; } = 1;
}