using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.Models;

namespace SiMax.Api.Services;

public class FinalPhaseQualificationService
{
    private readonly SiMaxDbContext _db;

    public FinalPhaseQualificationService(SiMaxDbContext db)
    {
        _db = db;
    }

    public async Task<List<FinalPhaseQualificationResult>> CalculateAsync(
        int tournamentId,
        int finalPhaseId)
    {
        var phase = await _db.FinalPhases
            .Include(fp => fp.Qualifications)
            .FirstOrDefaultAsync(fp =>
                fp.Id == finalPhaseId &&
                fp.TournamentId == tournamentId);

        if (phase == null)
            throw new InvalidOperationException("Final phase not found.");

        var pools = await _db.Pools
            .Where(p => p.TournamentId == tournamentId)
            .Include(p => p.Entries)
                .ThenInclude(e => e.Registration)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        if (pools.Count == 0)
            throw new InvalidOperationException(
                "The tournament has no pools.");

        var matches = await _db.Matches
            .Where(m =>
                m.TournamentId == tournamentId &&
                m.Status == "Completed" &&
                m.Team1Score.HasValue &&
                m.Team2Score.HasValue)
            .ToListAsync();

        var standings = CalculateStandings(pools, matches);

        var result = new List<FinalPhaseQualificationResult>();

        foreach (var rule in phase.Qualifications)
        {
            var candidates = standings
                .Where(s => s.Position == rule.PoolPosition)
                .ToList();

            if (rule.RuleType.Equals(
                    "Position",
                    StringComparison.OrdinalIgnoreCase))
            {
                result.AddRange(candidates);
            }
            else if (rule.RuleType.Equals(
                         "BestOfPosition",
                         StringComparison.OrdinalIgnoreCase))
            {
                result.AddRange(
                    candidates
                        .OrderByDescending(s => s.Wins)
                        .ThenByDescending(s => s.PointsFor)
                        .ThenByDescending(s => s.PointDifference)
                        .ThenBy(s => s.TeamName)
                        .Take(rule.Count));
            }
            else if (rule.RuleType.Equals(
                         "WorstOfPosition",
                         StringComparison.OrdinalIgnoreCase))
            {
                result.AddRange(
                    candidates
                        .OrderBy(s => s.Wins)
                        .ThenBy(s => s.PointsFor)
                        .ThenBy(s => s.PointDifference)
                        .ThenBy(s => s.TeamName)
                        .Take(rule.Count));
            }
        }

        // Una squadra non può comparire due volte
        // nella stessa fase.
        var duplicateTeams = result
            .GroupBy(x => x.RegistrationId)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicateTeams.Count > 0)
        {
            throw new InvalidOperationException(
                $"A team is qualified more than once in phase '{phase.Name}'.");
        }

        return result;
    }

    private static List<FinalPhaseQualificationResult> CalculateStandings(
        List<Pool> pools,
        List<Match> matches)
    {
        var result = new List<FinalPhaseQualificationResult>();

        foreach (var pool in pools)
        {
            var teams = pool.Entries
                .Select(e => new FinalPhaseQualificationResult
                {
                    RegistrationId = e.RegistrationId,
                    TeamName = e.Registration.TeamName,
                    PoolId = pool.Id,
                    PoolName = pool.Name
                })
                .ToList();

            foreach (var team in teams)
            {
                var teamMatches = matches
                    .Where(m =>
                        m.PoolId == pool.Id &&
                        (m.Team1RegistrationId == team.RegistrationId ||
                         m.Team2RegistrationId == team.RegistrationId))
                    .ToList();

                foreach (var match in teamMatches)
                {
                    var isTeam1 =
                        match.Team1RegistrationId == team.RegistrationId;

                    var ownScore = isTeam1
                        ? match.Team1Score!.Value
                        : match.Team2Score!.Value;

                    var opponentScore = isTeam1
                        ? match.Team2Score!.Value
                        : match.Team1Score!.Value;

                    team.MatchesPlayed++;

                    team.PointsFor += ownScore;
                    team.PointsAgainst += opponentScore;

                    if (ownScore > opponentScore)
                        team.Wins++;
                    else
                        team.Losses++;
                }

                team.PointDifference =
                    team.PointsFor - team.PointsAgainst;
            }

            var ordered = teams
                .OrderByDescending(t => t.Wins)
                .ThenByDescending(t => t.PointsFor)
                .ThenByDescending(t => t.PointDifference)
                .ThenBy(t => t.TeamName)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Position = i + 1;
            }

            result.AddRange(ordered);
        }

        return result;
    }
}

public class FinalPhaseQualificationResult
{
    public int RegistrationId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public int PoolId { get; set; }

    public string PoolName { get; set; } = string.Empty;

    public int Position { get; set; }

    public int MatchesPlayed { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int PointsFor { get; set; }

    public int PointsAgainst { get; set; }

    public int PointDifference { get; set; }
}