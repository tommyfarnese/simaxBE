namespace SiMax.Api.DTOs.Admin;

public class PoolPreviewDto
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<PoolPreviewEntryDto> Teams { get; set; } = new();
}