using SiMax.Api.Models;

namespace SiMax.Api.Services;

public class FinalBracketGenerator
{
    public List<GeneratedFinalMatch> Generate(
        FinalPhase phase,
        List<FinalPhaseQualificationResult> qualifiers)
    {
        if (!string.Equals(
                phase.EliminationType,
                "SingleElimination",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported elimination type: {phase.EliminationType}");
        }

        if (qualifiers.Count < 2)
        {
            throw new InvalidOperationException(
                "At least two teams are required.");
        }

        if (!IsPowerOfTwo(qualifiers.Count))
        {
            throw new InvalidOperationException(
                "The number of qualified teams must be a power of two.");
        }

        // Troviamo gli accoppiamenti del primo turno.
        var firstRoundPairings = BuildFirstRoundPairings(qualifiers);

        var matches = new List<GeneratedFinalMatch>();

        // Primo turno
        for (var i = 0; i < firstRoundPairings.Count; i++)
        {
            var pairing = firstRoundPairings[i];

            matches.Add(new GeneratedFinalMatch
            {
                RoundNumber = 1,
                MatchNumber = i + 1,
                Team1RegistrationId = pairing.Team1.RegistrationId,
                Team2RegistrationId = pairing.Team2.RegistrationId
            });
        }

        // Turni successivi.
        var previousRoundMatches = firstRoundPairings.Count;
        var roundNumber = 2;

        while (previousRoundMatches > 1)
        {
            var previousMatches = matches
                .Where(m => m.RoundNumber == roundNumber - 1)
                .OrderBy(m => m.MatchNumber)
                .ToList();

            var currentRoundMatches = previousRoundMatches / 2;

            for (var i = 0; i < currentRoundMatches; i++)
            {
                var team1Source = previousMatches[i * 2];
                var team2Source = previousMatches[i * 2 + 1];

                matches.Add(new GeneratedFinalMatch
                {
                    RoundNumber = roundNumber,
                    MatchNumber = i + 1,

                    Team1SourceMatchNumber =
                        team1Source.MatchNumber,

                    Team2SourceMatchNumber =
                        team2Source.MatchNumber
                });
            }

            previousRoundMatches = currentRoundMatches;
            roundNumber++;
        }

        return matches;
    }

    private static List<FinalMatchPairing> BuildFirstRoundPairings(
        List<FinalPhaseQualificationResult> qualifiers)
    {
        /*
         * Cerchiamo l'accoppiamento migliore usando backtracking.
         *
         * Priorità:
         *
         * 1. Gironi diversi
         * 2. 1ª contro 1ª vietato
         * 3. Preferenza 1ª contro 2ª
         * 4. Evitare, quando possibile, stesso girone
         */

        var teams = qualifiers
            .OrderBy(q => q.PoolId)
            .ThenBy(q => q.Position)
            .ThenBy(q => q.TeamName)
            .ToList();

        var pairings = new List<FinalMatchPairing>();

        if (TryBuildPairings(
                teams,
                new HashSet<int>(),
                pairings))
        {
            return pairings;
        }

        /*
         * Non dovrebbe normalmente accadere perché le configurazioni
         * previste dal torneo devono permettere gli incroci.
         *
         * Come fallback costruiamo comunque il tabellone evitando,
         * per quanto possibile, 1ª vs 1ª.
         */

        return BuildFallbackPairings(teams);
    }

    private static bool TryBuildPairings(
        List<FinalPhaseQualificationResult> teams,
        HashSet<int> used,
        List<FinalMatchPairing> pairings)
    {
        if (used.Count == teams.Count)
            return true;

        var firstIndex = -1;

        for (var i = 0; i < teams.Count; i++)
        {
            if (!used.Contains(teams[i].RegistrationId))
            {
                firstIndex = i;
                break;
            }
        }

        if (firstIndex < 0)
            return true;

        var team1 = teams[firstIndex];

        var candidates = teams
            .Select((team, index) => new
            {
                Team = team,
                Index = index
            })
            .Where(x =>
                !used.Contains(x.Team.RegistrationId) &&
                x.Index != firstIndex)
            .OrderByDescending(x =>
                GetPairingScore(team1, x.Team))
            .ThenBy(x => x.Team.PoolId)
            .ThenBy(x => x.Team.Position)
            .ThenBy(x => x.Team.TeamName)
            .ToList();

        foreach (var candidate in candidates)
        {
            var team2 = candidate.Team;

            /*
             * Regola assoluta:
             *
             * una prima classificata non può incontrare
             * un'altra prima classificata al primo turno.
             */
            if (team1.Position == 1 &&
                team2.Position == 1)
            {
                continue;
            }

            /*
             * Prima proviamo a non mettere nello stesso
             * incontro due squadre dello stesso girone.
             */
            if (team1.PoolId == team2.PoolId &&
                ExistsAlternativeOpponent(
                    team1,
                    team2,
                    teams,
                    used))
            {
                continue;
            }

            used.Add(team1.RegistrationId);
            used.Add(team2.RegistrationId);

            pairings.Add(new FinalMatchPairing
            {
                Team1 = team1,
                Team2 = team2
            });

            if (TryBuildPairings(teams, used, pairings))
                return true;

            pairings.RemoveAt(pairings.Count - 1);

            used.Remove(team1.RegistrationId);
            used.Remove(team2.RegistrationId);
        }

        return false;
    }

    private static bool ExistsAlternativeOpponent(
        FinalPhaseQualificationResult team1,
        FinalPhaseQualificationResult currentOpponent,
        List<FinalPhaseQualificationResult> teams,
        HashSet<int> used)
    {
        return teams.Any(other =>
            !used.Contains(other.RegistrationId) &&
            other.RegistrationId != team1.RegistrationId &&
            other.RegistrationId != currentOpponent.RegistrationId &&
            other.PoolId != team1.PoolId &&
            !(team1.Position == 1 && other.Position == 1));
    }

    private static int GetPairingScore(
        FinalPhaseQualificationResult team1,
        FinalPhaseQualificationResult team2)
    {
        var score = 0;

        // Priorità assoluta agli incroci tra gironi.
        if (team1.PoolId != team2.PoolId)
            score += 1000;

        // Prima contro seconda è l'incrocio preferito.
        if ((team1.Position == 1 && team2.Position == 2) ||
            (team1.Position == 2 && team2.Position == 1))
        {
            score += 500;
        }

        // Prima contro posizione superiore a 2:
        // comunque meglio di prima contro prima.
        if (team1.Position == 1 || team2.Position == 1)
            score += 100;

        // Posizioni diverse sono preferite.
        if (team1.Position != team2.Position)
            score += 50;

        return score;
    }

    private static List<FinalMatchPairing> BuildFallbackPairings(
        List<FinalPhaseQualificationResult> teams)
    {
        var result = new List<FinalMatchPairing>();
        var remaining = teams.ToList();

        while (remaining.Count > 0)
        {
            var team1 = remaining[0];

            var opponent = remaining
                .Skip(1)
                .Where(t =>
                    !(team1.Position == 1 && t.Position == 1))
                .OrderByDescending(t =>
                    GetPairingScore(team1, t))
                .ThenBy(t => t.PoolId == team1.PoolId)
                .ThenBy(t => t.Position)
                .ThenBy(t => t.TeamName)
                .FirstOrDefault();

            if (opponent == null)
            {
                // Situazione estremamente particolare:
                // non esiste più un avversario compatibile.
                opponent = remaining[1];
            }

            result.Add(new FinalMatchPairing
            {
                Team1 = team1,
                Team2 = opponent
            });

            remaining.Remove(team1);
            remaining.Remove(opponent);
        }

        return result;
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }
}

public class FinalMatchPairing
{
    public FinalPhaseQualificationResult Team1 { get; set; } = null!;
    public FinalPhaseQualificationResult Team2 { get; set; } = null!;
}

public class GeneratedFinalMatch
{
    public int RoundNumber { get; set; }

    public int MatchNumber { get; set; }

    public int? Team1RegistrationId { get; set; }

    public int? Team2RegistrationId { get; set; }

    public int? Team1SourceMatchNumber { get; set; }

    public int? Team2SourceMatchNumber { get; set; }
}