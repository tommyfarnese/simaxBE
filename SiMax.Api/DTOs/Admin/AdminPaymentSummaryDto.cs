namespace SiMax.Api.DTOs.Admin;

public class AdminPaymentSummaryDto
{
    public int TotalAmount { get; set; }

    public int MassimoAmount { get; set; }

    public int SilviaAmount { get; set; }

    public decimal Difference { get; set; }

    public string Settlement { get; set; } = string.Empty;
}
