using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;
using SiMax.Api.Services;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/tournaments/{tournamentId}/final-phases")]
[Authorize(Policy = "AdminOnly")]
public class AdminFinalPhasesController : ControllerBase
{
    private readonly SiMaxDbContext _db;

    public AdminFinalPhasesController(SiMaxDbContext db)
    {
        _db = db;
    }

    // GET: /api/admin/tournaments/{tournamentId}/final-phases
    [HttpGet]
    public async Task<ActionResult<List<FinalPhaseDto>>> Get(int tournamentId)
    {
        var tournamentExists = await _db.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
            return NotFound("Tournament not found.");

        var phases = await _db.FinalPhases
            .Where(fp => fp.TournamentId == tournamentId)
            .Include(fp => fp.Qualifications)
            .OrderBy(fp => fp.Id)
            .ToListAsync();

        var poolCount = await _db.Pools
    .CountAsync(p => p.TournamentId == tournamentId);

        var result = phases.Select(fp => new FinalPhaseDto
        {
            Id = fp.Id,
            Name = fp.Name,
            EliminationType = fp.EliminationType,

            Rules = fp.Qualifications
                .OrderBy(q => q.PoolPosition)
                .ThenBy(q => q.RuleType)
                .Select(q => new FinalPhaseQualificationDto
                {
                    RuleType = q.RuleType,
                    PoolPosition = q.PoolPosition,
                    Count = q.Count
                })
                .ToList(),

            QualifiedTeamsCount = fp.Qualifications.Sum(q =>
                q.RuleType.Equals(
                    "Position",
                    StringComparison.OrdinalIgnoreCase)
                        ? poolCount
                        : q.Count)
        }).ToList();

        return Ok(result);
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    // PUT: /api/admin/tournaments/{tournamentId}/final-phases
    [HttpPut]
    public async Task<ActionResult<List<FinalPhaseDto>>> Update(
        int tournamentId,
        FinalPhaseConfigurationRequest request)
    {
        var tournamentExists = await _db.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
            return NotFound("Tournament not found.");

        if (request.Phases.Count == 0)
            return BadRequest("At least one final phase is required.");

        var phaseNames = request.Phases
            .Select(p => p.Name.Trim())
            .ToList();

        if (phaseNames.Any(string.IsNullOrWhiteSpace))
            return BadRequest("Phase name is required.");

        if (phaseNames
            .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1))
        {
            return BadRequest("Phase names must be unique.");
        }

        // Recuperiamo i gironi del torneo
        var pools = await _db.Pools
            .Where(p => p.TournamentId == tournamentId)
            .Include(p => p.Entries)
            .ToListAsync();

        if (pools.Count == 0)
        {
            return BadRequest(
                "The tournament has no pools configured.");
        }

        var maxPoolSize = pools.Max(p => p.Entries.Count);

        // Validazione delle fasi
        foreach (var phase in request.Phases)
        {
            if (phase.Rules.Count == 0)
            {
                return BadRequest(
                    $"Phase '{phase.Name}' must have at least one qualification rule.");
            }

            if (!string.Equals(
                    phase.EliminationType,
                    "SingleElimination",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    $"Unsupported elimination type: {phase.EliminationType}");
            }

            foreach (var rule in phase.Rules)
            {
                // Tipo di regola
                if (!string.Equals(
                        rule.RuleType,
                        "Position",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        rule.RuleType,
                        "BestOfPosition",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        rule.RuleType,
                        "WorstOfPosition",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(
                        $"Unsupported qualification rule type: {rule.RuleType}");
                }

                // Posizione valida
                if (rule.PoolPosition < 1)
                {
                    return BadRequest(
                        $"Invalid pool position in phase '{phase.Name}'.");
                }

                if (rule.PoolPosition > maxPoolSize)
                {
                    return BadRequest(
                        $"Position {rule.PoolPosition} does not exist " +
                        $"in the tournament pools.");
                }

                // Count valido
                if (rule.Count < 1)
                {
                    return BadRequest(
                        $"Count must be greater than zero in phase '{phase.Name}'.");
                }

                // Position significa:
                // una squadra per ogni girone.
                if (string.Equals(
                        rule.RuleType,
                        "Position",
                        StringComparison.OrdinalIgnoreCase) &&
                    rule.Count != 1)
                {
                    return BadRequest(
                        $"A Position rule must have Count = 1 in phase '{phase.Name}'.");
                }

                // Best/Worst possono scegliere al massimo
                // una squadra per ogni girone.
                if ((string.Equals(
                        rule.RuleType,
                        "BestOfPosition",
                        StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(
                        rule.RuleType,
                        "WorstOfPosition",
                        StringComparison.OrdinalIgnoreCase)) &&
                    rule.Count > pools.Count)
                {
                    return BadRequest(
                        $"Rule '{rule.RuleType}' for position " +
                        $"{rule.PoolPosition} requests {rule.Count} teams, " +
                        $"but there are only {pools.Count} pools.");
                }
            }

            // La stessa posizione non può essere usata
            // da più regole nella stessa fase.
            var duplicatedPositions = phase.Rules
                .GroupBy(r => r.PoolPosition)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedPositions.Count > 0)
            {
                return BadRequest(
                    $"Position(s) {string.Join(", ", duplicatedPositions)} " +
                    $"are used by multiple rules in phase '{phase.Name}'.");
            }

            // Calcoliamo quante squadre accederanno alla fase.
            var qualifiedTeamsCount = phase.Rules.Sum(rule =>
            {
                if (string.Equals(
                        rule.RuleType,
                        "Position",
                        StringComparison.OrdinalIgnoreCase))
                {
                    // Una squadra per ogni girone.
                    return pools.Count;
                }

                // BestOfPosition / WorstOfPosition:
                // Count squadre complessivamente.
                return rule.Count;
            });

            // Il tabellone Single Elimination deve avere
            // 2, 4, 8, 16, 32... squadre.
            if (!IsPowerOfTwo(qualifiedTeamsCount))
            {
                return BadRequest(
                    $"Phase '{phase.Name}' would have " +
                    $"{qualifiedTeamsCount} qualified teams. " +
                    "The number of qualified teams must be a power of two " +
                    "(2, 4, 8, 16, ...).");
            }
        }

        // Salviamo la nuova configurazione in una transazione.
        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var existingPhases = await _db.FinalPhases
            .Where(fp => fp.TournamentId == tournamentId)
            .Include(fp => fp.Qualifications)
            .ToListAsync();

        if (existingPhases.Count > 0)
        {
            _db.FinalPhases.RemoveRange(existingPhases);
        }

        foreach (var phaseRequest in request.Phases)
        {
            var phase = new FinalPhase
            {
                TournamentId = tournamentId,
                Name = phaseRequest.Name.Trim(),
                EliminationType = phaseRequest.EliminationType.Trim()
            };

            foreach (var ruleRequest in phaseRequest.Rules)
            {
                phase.Qualifications.Add(
                    new FinalPhaseQualification
                    {
                        RuleType = ruleRequest.RuleType.Trim(),
                        PoolPosition = ruleRequest.PoolPosition,
                        Count = ruleRequest.Count
                    });
            }

            _db.FinalPhases.Add(phase);
        }

        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        return await Get(tournamentId);
    }

    // GET: /api/admin/tournaments/{tournamentId}/final-phases/{phaseId}/qualifiers
    [HttpGet("{phaseId}/qualifiers")]
    public async Task<ActionResult<List<FinalPhaseQualifierDto>>> GetQualifiers(
        int tournamentId,
        int phaseId,
        [FromServices] FinalPhaseQualificationService qualificationService)
    {
        var phaseExists = await _db.FinalPhases
            .AnyAsync(fp =>
                fp.Id == phaseId &&
                fp.TournamentId == tournamentId);

        if (!phaseExists)
            return NotFound("Final phase not found.");

        try
        {
            var qualifiers = await qualificationService.CalculateAsync(
                tournamentId,
                phaseId);

            var result = qualifiers
                .Select(q => new FinalPhaseQualifierDto
                {
                    RegistrationId = q.RegistrationId,
                    TeamName = q.TeamName,
                    PoolId = q.PoolId,
                    PoolName = q.PoolName,
                    Position = q.Position,
                    MatchesPlayed = q.MatchesPlayed,
                    Wins = q.Wins,
                    Losses = q.Losses,
                    PointsFor = q.PointsFor,
                    PointsAgainst = q.PointsAgainst,
                    PointDifference = q.PointDifference
                })
                .ToList();

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: /api/admin/tournaments/{tournamentId}/final-phases/{phaseId}/generate
    [HttpPost("{phaseId}/generate")]
    public async Task<ActionResult<List<FinalMatchDto>>> GenerateBracket(
        int tournamentId,
        int phaseId,
        [FromServices] FinalPhaseQualificationService qualificationService,
        [FromServices] FinalBracketGenerator bracketGenerator)
    {
        var phase = await _db.FinalPhases
            .FirstOrDefaultAsync(fp =>
                fp.Id == phaseId &&
                fp.TournamentId == tournamentId);

        if (phase == null)
            return NotFound("Final phase not found.");

        try
        {
            // Calcoliamo le squadre qualificate
            var qualifiers = await qualificationService.CalculateAsync(
                tournamentId,
                phaseId);

            // Generiamo la struttura del tabellone
            var generatedMatches = bracketGenerator.Generate(
                phase,
                qualifiers);

            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            // Eliminiamo un eventuale tabellone precedente
            var existingMatches = await _db.FinalMatches
                .Where(m => m.FinalPhaseId == phaseId)
                .ToListAsync();

            if (existingMatches.Count > 0)
            {
                _db.FinalMatches.RemoveRange(existingMatches);
                await _db.SaveChangesAsync();
            }

            /*
             * Prima salviamo tutte le partite.
             *
             * I match dei turni successivi hanno ancora
             * Team1SourceMatchNumber / Team2SourceMatchNumber.
             *
             * Dopo il salvataggio abbiamo gli ID reali dei FinalMatch
             * e possiamo collegare correttamente le partite precedenti.
             */

            var dbMatches = generatedMatches
                .Select(m => new FinalMatch
                {
                    FinalPhaseId = phaseId,
                    RoundNumber = m.RoundNumber,
                    MatchNumber = m.MatchNumber,
                    Team1RegistrationId = m.Team1RegistrationId,
                    Team2RegistrationId = m.Team2RegistrationId,
                    Status = "Scheduled"
                })
                .ToList();

            _db.FinalMatches.AddRange(dbMatches);

            await _db.SaveChangesAsync();

            /*
             * Creiamo una mappa:
             *
             * Round 1 / Match 1 -> ID database
             * Round 1 / Match 2 -> ID database
             * ...
             */

            var matchMap = dbMatches.ToDictionary(
                m => (m.RoundNumber, m.MatchNumber),
                m => m.Id);

            /*
             * Ora colleghiamo i match dei turni successivi
             * ai match dai quali arriveranno le squadre.
             */

            foreach (var generated in generatedMatches)
            {
                if (generated.Team1SourceMatchNumber.HasValue)
                {
                    var sourceKey = (
                        generated.RoundNumber - 1,
                        generated.Team1SourceMatchNumber.Value);

                    if (!matchMap.TryGetValue(sourceKey, out var sourceMatchId))
                    {
                        throw new InvalidOperationException(
                            "Unable to resolve Team1 source match.");
                    }

                    var currentMatch = dbMatches.First(m =>
                        m.RoundNumber == generated.RoundNumber &&
                        m.MatchNumber == generated.MatchNumber);

                    currentMatch.Team1SourceMatchId = sourceMatchId;
                }

                if (generated.Team2SourceMatchNumber.HasValue)
                {
                    var sourceKey = (
                        generated.RoundNumber - 1,
                        generated.Team2SourceMatchNumber.Value);

                    if (!matchMap.TryGetValue(sourceKey, out var sourceMatchId))
                    {
                        throw new InvalidOperationException(
                            "Unable to resolve Team2 source match.");
                    }

                    var currentMatch = dbMatches.First(m =>
                        m.RoundNumber == generated.RoundNumber &&
                        m.MatchNumber == generated.MatchNumber);

                    currentMatch.Team2SourceMatchId = sourceMatchId;
                }
            }

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(
                dbMatches
                    .OrderBy(m => m.RoundNumber)
                    .ThenBy(m => m.MatchNumber)
                    .Select(m => new FinalMatchDto
                    {
                        Id = m.Id,
                        FinalPhaseId = m.FinalPhaseId,
                        RoundNumber = m.RoundNumber,
                        MatchNumber = m.MatchNumber,
                        CourtId = m.CourtId,
                        StartTime = m.StartTime,
                        EndTime = m.EndTime,
                        Team1RegistrationId = m.Team1RegistrationId,
                        Team2RegistrationId = m.Team2RegistrationId,
                        Team1Score = m.Team1Score,
                        Team2Score = m.Team2Score,
                        Status = m.Status,
                        Team1SourceMatchId = m.Team1SourceMatchId,
                        Team2SourceMatchId = m.Team2SourceMatchId
                    })
                    .ToList());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: /api/admin/tournaments/{tournamentId}/final-phases/{phaseId}/matches/{matchId}/result
    [HttpPut("{phaseId}/matches/{matchId}/result")]
    public async Task<ActionResult<FinalMatchDto>> SetFinalMatchResult(
        int tournamentId,
        int phaseId,
        int matchId,
        FinalMatchResultRequest request)
    {
        var match = await _db.FinalMatches
            .FirstOrDefaultAsync(m =>
                m.Id == matchId &&
                m.FinalPhaseId == phaseId &&
                m.FinalPhase.TournamentId == tournamentId);

        if (match == null)
            return NotFound("Final match not found.");

        if (match.Status == "Cancelled")
            return BadRequest("A cancelled match cannot receive a result.");

        if (!match.Team1RegistrationId.HasValue ||
            !match.Team2RegistrationId.HasValue)
        {
            return BadRequest(
                "Both teams must be determined before entering the result.");
        }

        if (request.Team1Score < 0 ||
            request.Team2Score < 0)
        {
            return BadRequest("Scores cannot be negative.");
        }

        if (request.Team1Score == request.Team2Score)
        {
            return BadRequest(
                "A final phase match cannot end in a draw.");
        }

        // Cerchiamo l'eventuale match successivo.
        var nextMatch = await _db.FinalMatches
            .FirstOrDefaultAsync(m =>
                m.FinalPhaseId == phaseId &&
                (
                    m.Team1SourceMatchId == match.Id ||
                    m.Team2SourceMatchId == match.Id
                ));

        var winnerId =
            request.Team1Score > request.Team2Score
                ? match.Team1RegistrationId.Value
                : match.Team2RegistrationId.Value;

        /*
         * Se questo match era già completato e il suo vincitore
         * era già stato inserito nel turno successivo, controlliamo
         * che non ci siano problemi.
         */
        if (match.Status == "Completed" &&
            nextMatch != null)
        {
            var oldWinnerId =
                nextMatch.Team1SourceMatchId == match.Id
                    ? nextMatch.Team1RegistrationId
                    : nextMatch.Team2RegistrationId;

            if (oldWinnerId.HasValue &&
                oldWinnerId.Value != winnerId)
            {
                return BadRequest(
                    "The result cannot be changed because the previous winner " +
                    "has already been propagated to the next match.");
            }
        }

        match.Team1Score = request.Team1Score;
        match.Team2Score = request.Team2Score;
        match.Status = "Completed";

        /*
         * Se esiste un turno successivo, inseriamo automaticamente
         * il vincitore nella posizione corretta.
         */
        if (nextMatch != null)
        {
            if (nextMatch.Team1SourceMatchId == match.Id)
            {
                nextMatch.Team1RegistrationId = winnerId;
            }
            else if (nextMatch.Team2SourceMatchId == match.Id)
            {
                nextMatch.Team2RegistrationId = winnerId;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new FinalMatchDto
        {
            Id = match.Id,
            FinalPhaseId = match.FinalPhaseId,
            RoundNumber = match.RoundNumber,
            MatchNumber = match.MatchNumber,
            CourtId = match.CourtId,
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            Team1RegistrationId = match.Team1RegistrationId,
            Team2RegistrationId = match.Team2RegistrationId,
            Team1Score = match.Team1Score,
            Team2Score = match.Team2Score,
            Status = match.Status,
            Team1SourceMatchId = match.Team1SourceMatchId,
            Team2SourceMatchId = match.Team2SourceMatchId
        });
    }
}