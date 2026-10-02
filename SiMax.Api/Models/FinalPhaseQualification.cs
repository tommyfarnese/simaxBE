namespace SiMax.Api.Models;

public class FinalPhaseQualification
{
    public int Id { get; set; }

    public int FinalPhaseId { get; set; }

    // Position = una specifica posizione del girone
    // BestOfPosition = le migliori N squadre di quella posizione
    // WorstOfPosition = le peggiori N squadre di quella posizione
    public string RuleType { get; set; } = "Position";

    // Posizione nella classifica del girone
    public int PoolPosition { get; set; }

    // Utilizzato per BestOfPosition / WorstOfPosition
    public int Count { get; set; } = 1;

    public FinalPhase FinalPhase { get; set; } = null!;
}