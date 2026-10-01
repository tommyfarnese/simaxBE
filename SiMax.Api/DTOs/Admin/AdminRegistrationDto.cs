namespace SiMax.Api.DTOs.Admin;

public class AdminRegistrationDto
{
    public int Id { get; set; }
    public int TournamentId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;

    public string Player1FirstName { get; set; } = string.Empty;
    public string Player1LastName { get; set; } = string.Empty;
    public string Player1Phone { get; set; } = string.Empty;

    public string Player2FirstName { get; set; } = string.Empty;
    public string Player2LastName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int AmountDue { get; set; }
    public int AmountPaid { get; set; }
    public int AmountRemaining { get; set; }

    public List<AdminPaymentDto> Payments { get; set; } = new();
}