using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.Models;

namespace SiMax.Api.Services;

public class FinalMatchScheduler
{
    private readonly SiMaxDbContext _db;

    private static readonly TimeSpan MatchDuration =
        TimeSpan.FromMinutes(20);

    private static readonly TimeSpan SlotDuration =
        TimeSpan.FromMinutes(20);

    public FinalMatchScheduler(SiMaxDbContext db)
    {
        _db = db;
    }

    public async Task<FinalMatchSchedulingResult> ScheduleAsync(
        int tournamentId,
        int phaseId)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return Failure("Tournament not found.");
        }

        var phase = await _db.FinalPhases
            .FirstOrDefaultAsync(fp =>
                fp.Id == phaseId &&
                fp.TournamentId == tournamentId);

        if (phase == null)
        {
            return Failure("Final phase not found.");
        }

        var matches = await _db.FinalMatches
            .Where(m => m.FinalPhaseId == phaseId)
            .OrderBy(m => m.RoundNumber)
            .ThenBy(m => m.MatchNumber)
            .ToListAsync();

        if (matches.Count == 0)
        {
            return Failure(
                "The final phase has no matches. Generate the bracket first.");
        }

        var courts = await _db.Courts
                .Where(c =>
                    c.IsActive &&
                    c.TournamentCourts.Any(tc =>
                        tc.TournamentId == tournamentId))
                .Include(c => c.Availabilities)
                .ToListAsync();

        if (courts.Count == 0)
        {
            return Failure(
                "No active courts are assigned to this tournament.");
        }

        /*
         * Partiamo dall'orario di inizio del torneo.
         *
         * Le finali potranno essere programmate solo
         * negli slot disponibili successivi.
         */
        var eventDate = tournament.Event.Date.Date;

        var existingMatches = await _db.Matches
            .Where(m =>
                m.TournamentId == tournamentId &&
                m.StartTime.Date == eventDate)
            .ToListAsync();

        /*
         * Le finali già programmate vengono ignorate e
         * riprogrammate completamente.
         */
        var finalMatchIds = matches
            .Select(m => m.Id)
            .ToHashSet();

        /*
         * Occupazione dei campi:
         *
         * - partite dei gironi già esistenti
         * - finali che aggiungiamo durante questa procedura
         */
        var occupiedSlots = new List<ScheduledSlot>();

        foreach (var match in existingMatches)
        {
            occupiedSlots.Add(new ScheduledSlot
            {
                CourtId = match.CourtId,
                StartTime = match.StartTime,
                EndTime = match.EndTime
            });
        }

        /*
         * Per evitare che una semifinale venga programmata
         * prima dei suoi quarti, lavoriamo per round.
         */
        var rounds = matches
            .GroupBy(m => m.RoundNumber)
            .OrderBy(g => g.Key)
            .ToList();

        /*
         * Prima cancelliamo la programmazione precedente
         * delle finali.
         */
        foreach (var match in matches)
        {
            match.CourtId = null;
            match.StartTime = null;
            match.EndTime = null;
        }

        /*
         * La prima partita delle finali non può partire
         * prima dell'inizio del torneo.
         */
        var earliestTime = eventDate
            .Add(tournament.StartTime);

        foreach (var round in rounds)
        {
            var previousRound = matches
                .Where(m => m.RoundNumber == round.Key - 1)
                .ToList();

            foreach (var match in round
                         .OrderBy(m => m.MatchNumber))
            {
                /*
                 * Per il primo turno possiamo partire dall'orario
                 * iniziale del torneo.
                 *
                 * Per i turni successivi dobbiamo aspettare
                 * la fine dei match che alimentano questa partita.
                 */
                var earliestForMatch = earliestTime;

                if (match.RoundNumber > 1)
                {
                    var sourceMatchIds = new List<int>();

                    if (match.Team1SourceMatchId.HasValue)
                        sourceMatchIds.Add(
                            match.Team1SourceMatchId.Value);

                    if (match.Team2SourceMatchId.HasValue)
                        sourceMatchIds.Add(
                            match.Team2SourceMatchId.Value);

                    var sourceMatches = matches
                        .Where(m => sourceMatchIds.Contains(m.Id))
                        .ToList();

                    if (sourceMatches.Count != sourceMatchIds.Count)
                    {
                        return Failure(
                            $"Unable to resolve source matches for " +
                            $"final match {match.Id}.");
                    }

                    if (sourceMatches.Any(m =>
                            !m.EndTime.HasValue))
                    {
                        return Failure(
                            $"Unable to schedule final match {match.Id}: " +
                            "one of its source matches has no scheduled time.");
                    }

                    var latestSourceEnd = sourceMatches
                        .Max(m => m.EndTime!.Value);

                    earliestForMatch =
                        latestSourceEnd;
                }

                var slot = FindAvailableSlot(
                    courts,
                    occupiedSlots,
                    earliestForMatch);

                if (slot == null)
                {
                    return Failure(
                        $"Unable to schedule final match " +
                        $"{match.MatchNumber} of round " +
                        $"{match.RoundNumber}.");
                }

                match.CourtId = slot.CourtId;
                match.StartTime = slot.StartTime;
                match.EndTime = slot.EndTime;

                occupiedSlots.Add(new ScheduledSlot
                {
                    CourtId = slot.CourtId,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                });
            }
        }

        return new FinalMatchSchedulingResult
        {
            Success = true,
            Matches = matches
        };
    }

    private static ScheduledSlot? FindAvailableSlot(
        List<Court> courts,
        List<ScheduledSlot> occupiedSlots,
        DateTime earliestTime)
    {
        /*
         * Cerchiamo il primo slot possibile,
         * ordinando prima per orario e poi per campo.
         */
        var candidates = new List<ScheduledSlot>();

        foreach (var court in courts)
        {
            foreach (var availability in court.Availabilities)
            {
                var availabilityStart =
                    earliestTime.Date.Add(
                        availability.StartTime);

                var availabilityEnd =
                    earliestTime.Date.Add(
                        availability.EndTime);

                var candidateStart = RoundUpToSlot(
                    earliestTime,
                    availabilityStart);

                while (candidateStart + MatchDuration <= availabilityEnd)
                {
                    var candidateEnd =
                        candidateStart + MatchDuration;

                    var overlaps = occupiedSlots.Any(
                        occupied =>
                            occupied.CourtId == court.Id &&
                            candidateStart < occupied.EndTime &&
                            candidateEnd > occupied.StartTime);

                    if (!overlaps)
                    {
                        candidates.Add(new ScheduledSlot
                        {
                            CourtId = court.Id,
                            StartTime = candidateStart,
                            EndTime = candidateEnd
                        });

                        break;
                    }

                    candidateStart += SlotDuration;
                }
            }
        }

        return candidates
            .OrderBy(c => c.StartTime)
            .ThenBy(c => c.CourtId)
            .FirstOrDefault();
    }

    private static DateTime RoundUpToSlot(
    DateTime value,
    DateTime availabilityStart)
    {
        var start = value > availabilityStart
            ? value
            : availabilityStart;

        var elapsedMinutes =
            (start - availabilityStart).TotalMinutes;

        var roundedIntervals =
            Math.Ceiling(elapsedMinutes / 20.0);

        return availabilityStart.AddMinutes(
            roundedIntervals * 20);
    }

    private static FinalMatchSchedulingResult Failure(
        string message)
    {
        return new FinalMatchSchedulingResult
        {
            Success = false,
            ErrorMessage = message
        };
    }
}

public class FinalMatchSchedulingResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public List<FinalMatch> Matches { get; set; } = new();
}

public class ScheduledSlot
{
    public int CourtId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}