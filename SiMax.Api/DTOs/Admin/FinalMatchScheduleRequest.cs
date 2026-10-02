using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class FinalMatchScheduleRequest
{
    [Required]
    public int CourtId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }
}