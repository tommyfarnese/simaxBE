namespace SiMax.Api.DTOs.Admin;

public class FinalPhaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EliminationType { get; set; } = string.Empty;
    public List<FinalPhaseQualificationDto> Rules { get; set; } = new();
    public int QualifiedTeamsCount { get; set; }
}

public class FinalPhaseQualificationDto
{
    public string RuleType { get; set; } = string.Empty;
    public int PoolPosition { get; set; }
    public int Count { get; set; }
}