using System.ComponentModel.DataAnnotations;

namespace SiMax.Api.DTOs.Admin;

public class UpdateTournamentCourtsRequest
{
    [Required]
    public List<int> CourtIds { get; set; } = new();
}