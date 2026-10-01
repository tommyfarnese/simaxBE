using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class AdminRegistrationStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}