namespace SiMax.Api.Models;

public class FinalPhase
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string EliminationType { get; set; } = "SingleElimination";

    public Tournament Tournament { get; set; } = null!;

    public ICollection<FinalPhaseQualification> Qualifications { get; set; }
        = new List<FinalPhaseQualification>();
}