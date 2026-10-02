namespace SiMax.Api.DTOs.Admin;

public class AdminParticipantPriceDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int TournamentCount { get; set; }

    public int AmountDue { get; set; }
    public int AmountPaid { get; set; }
    public int AmountRemaining { get; set; }

    public List<int> TournamentIds { get; set; } = new();
}