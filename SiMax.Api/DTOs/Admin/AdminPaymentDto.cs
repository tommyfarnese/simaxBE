namespace SiMax.Api.DTOs.Admin;

public class AdminPaymentDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public int PlayerNumber { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public int Amount { get; set; }
}