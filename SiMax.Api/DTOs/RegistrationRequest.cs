using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs;

public class RegistrationRequest
{
    [Required]
    public int TournamentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TeamName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Player1FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Player1LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Player1Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Player2FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Player2LastName { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;
}