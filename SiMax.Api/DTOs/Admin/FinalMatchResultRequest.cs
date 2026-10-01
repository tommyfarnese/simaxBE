using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class FinalMatchResultRequest
{
    [Range(0, 999)]
    public int Team1Score { get; set; }

    [Range(0, 999)]
    public int Team2Score { get; set; }
}