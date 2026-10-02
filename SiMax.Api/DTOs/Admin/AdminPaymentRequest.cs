using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class AdminPaymentRequest
{
    [Range(1, 2)]
    public int PlayerNumber { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    public string ReceivedBy { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Amount { get; set; }
}