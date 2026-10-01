namespace SiMax.Api.Models;

public class Payment
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }

    // 1 = Player 1, 2 = Player 2
    public int PlayerNumber { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string ReceivedBy { get; set; } = string.Empty;

    public int Amount { get; set; }

    public Registration Registration { get; set; } = null!;
}