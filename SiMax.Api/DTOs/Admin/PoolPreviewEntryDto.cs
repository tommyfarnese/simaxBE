namespace SiMax.Api.DTOs.Admin;

public class PoolPreviewEntryDto
{
    public int RegistrationId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Player1FirstName { get; set; } = string.Empty;
    public string Player1LastName { get; set; } = string.Empty;
    public string Player2FirstName { get; set; } = string.Empty;
    public string Player2LastName { get; set; } = string.Empty;
}