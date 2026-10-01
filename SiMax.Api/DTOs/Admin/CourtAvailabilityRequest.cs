using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class CourtAvailabilityRequest
{
    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}