using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class UpdateMatchResultRequest
{
    [Range(0, 100)]
    public int Team1Score { get; set; }

    [Range(0, 100)]
    public int Team2Score { get; set; }
}