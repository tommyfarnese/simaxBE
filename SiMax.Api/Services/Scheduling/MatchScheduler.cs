using SiMax.Api.Models;

namespace SiMax.Api.Services.Scheduling;

public class MatchScheduler
{
    private const int SlotDurationMinutes = 20;

    public MatchSchedulingResult Schedule(
        Tournament tournament,
        List<Pool> pools,
        List<Court> courts,
        List<Match> existingMatches)
    {
        var result = new MatchSchedulingResult();

        if (pools.Count == 0)
        {
            result.ErrorMessage = "Non ci sono gironi.";
            return result;
        }

        if (courts.Count == 0)
        {
            result.ErrorMessage =
                "Non ci sono campi configurati per questo torneo.";
            return result;
        }

        var generatedMatches = GenerateRoundRobinMatches(pools);

        if (generatedMatches.Count == 0)
        {
            result.ErrorMessage =
                "Non ci sono partite da programmare.";
            return result;
        }

        var tournamentStart =
            tournament.Event.Date.Date + tournament.StartTime;

        var tournamentEnd =
            tournament.Event.Date.Date + tournament.EndTime;

        if (tournamentEnd <= tournamentStart)
        {
            result.ErrorMessage =
                "L'orario di fine del torneo deve essere successivo " +
                "all'orario di inizio.";
            return result;
        }

        var slots = BuildSlots(
            tournamentStart,
            tournamentEnd);

        if (slots.Count == 0)
        {
            result.ErrorMessage =
                "Il torneo non contiene slot disponibili.";
            return result;
        }

        var occupiedCourts = existingMatches
            .Select(m => new CourtSlotKey(
                m.CourtId,
                m.StartTime))
            .ToHashSet();

        var occupiedTeams = existingMatches
            .SelectMany(m => new[]
            {
                new TeamSlotKey(
                    m.Team1RegistrationId,
                    m.StartTime),

                new TeamSlotKey(
                    m.Team2RegistrationId,
                    m.StartTime)
            })
            .ToHashSet();

        var scheduled = new List<ScheduledMatch>();

        var success = Backtrack(
            generatedMatches,
            scheduled,
            slots,
            courts,
            occupiedCourts,
            occupiedTeams);

        if (!success)
        {
            result.ErrorMessage =
                "Non è stato possibile creare una pianificazione " +
                "completa rispettando tutti i vincoli.";

            return result;
        }

        result.Success = true;
        result.Matches = scheduled
            .OrderBy(m => m.StartTime)
            .ThenBy(m => m.CourtId)
            .ToList();

        return result;
    }

    private bool Backtrack(
        List<GeneratedMatch> remainingMatches,
        List<ScheduledMatch> scheduled,
        List<TimeSlot> slots,
        List<Court> courts,
        HashSet<CourtSlotKey> occupiedCourts,
        HashSet<TeamSlotKey> occupiedTeams)
    {
        if (remainingMatches.Count == 0)
        {
            return true;
        }

        var nextMatch = SelectNextMatch(
            remainingMatches,
            slots,
            courts,
            scheduled,
            occupiedCourts,
            occupiedTeams);

        if (nextMatch == null)
        {
            return false;
        }

        var candidates = GetCandidates(
            nextMatch,
            slots,
            courts,
            scheduled,
            occupiedCourts,
            occupiedTeams);

        foreach (var candidate in candidates)
        {
            var scheduledMatch = new ScheduledMatch
            {
                PoolId = nextMatch.PoolId,
                Team1RegistrationId =
                    nextMatch.Team1RegistrationId,
                Team2RegistrationId =
                    nextMatch.Team2RegistrationId,
                CourtId = candidate.CourtId,
                SlotNumber = candidate.SlotNumber,
                StartTime = candidate.StartTime,
                EndTime = candidate.EndTime
            };

            scheduled.Add(scheduledMatch);

            occupiedCourts.Add(
                new CourtSlotKey(
                    candidate.CourtId,
                    candidate.StartTime));

            occupiedTeams.Add(
                new TeamSlotKey(
                    nextMatch.Team1RegistrationId,
                    candidate.StartTime));

            occupiedTeams.Add(
                new TeamSlotKey(
                    nextMatch.Team2RegistrationId,
                    candidate.StartTime));

            remainingMatches.Remove(nextMatch);

            if (Backtrack(
                remainingMatches,
                scheduled,
                slots,
                courts,
                occupiedCourts,
                occupiedTeams))
            {
                return true;
            }

            remainingMatches.Add(nextMatch);

            occupiedCourts.Remove(
                new CourtSlotKey(
                    candidate.CourtId,
                    candidate.StartTime));

            occupiedTeams.Remove(
                new TeamSlotKey(
                    nextMatch.Team1RegistrationId,
                    candidate.StartTime));

            occupiedTeams.Remove(
                new TeamSlotKey(
                    nextMatch.Team2RegistrationId,
                    candidate.StartTime));

            scheduled.Remove(scheduledMatch);
        }

        return false;
    }

    private GeneratedMatch? SelectNextMatch(
        List<GeneratedMatch> matches,
        List<TimeSlot> slots,
        List<Court> courts,
        List<ScheduledMatch> scheduled,
        HashSet<CourtSlotKey> occupiedCourts,
        HashSet<TeamSlotKey> occupiedTeams)
    {
        return matches
            .Select(match => new
            {
                Match = match,
                CandidateCount = GetCandidates(
                    match,
                    slots,
                    courts,
                    scheduled,
                    occupiedCourts,
                    occupiedTeams).Count
            })
            .OrderBy(x => x.CandidateCount)
            .ThenByDescending(x =>
                GetEarliestRequiredSlot(x.Match))
            .Select(x => x.Match)
            .FirstOrDefault();
    }

    private List<Candidate> GetCandidates(
        GeneratedMatch match,
        List<TimeSlot> slots,
        List<Court> courts,
        List<ScheduledMatch> scheduled,
        HashSet<CourtSlotKey> occupiedCourts,
        HashSet<TeamSlotKey> occupiedTeams)
    {
        var candidates = new List<Candidate>();

        var minimumSlot = Math.Max(
            GetEarliestRequiredSlot(match),
            1);

        foreach (var slot in slots.Where(s =>
                     s.SlotNumber >= minimumSlot))
        {
            var team1AlreadyPlaying =
                occupiedTeams.Contains(
                    new TeamSlotKey(
                        match.Team1RegistrationId,
                        slot.StartTime));

            var team2AlreadyPlaying =
                occupiedTeams.Contains(
                    new TeamSlotKey(
                        match.Team2RegistrationId,
                        slot.StartTime));

            if (team1AlreadyPlaying || team2AlreadyPlaying)
            {
                continue;
            }

            foreach (var court in courts)
            {
                if (!IsCourtAvailable(
                    court,
                    slot.StartTime,
                    slot.EndTime))
                {
                    continue;
                }

                var courtOccupied =
                    occupiedCourts.Contains(
                        new CourtSlotKey(
                            court.Id,
                            slot.StartTime));

                if (courtOccupied)
                {
                    continue;
                }

                var consecutivePenalty =
                    CalculateConsecutivePenalty(
                        match,
                        slot,
                        scheduled);

                candidates.Add(new Candidate
                {
                    CourtId = court.Id,
                    SlotNumber = slot.SlotNumber,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Penalty = consecutivePenalty
                });
            }
        }

        return candidates
            .OrderBy(c => c.Penalty)
            .ThenBy(c => c.StartTime)
            .ThenBy(c => c.CourtId)
            .ToList();
    }

    private static int CalculateConsecutivePenalty(
        GeneratedMatch match,
        TimeSlot slot,
        List<ScheduledMatch> scheduled)
    {
        var penalty = 0;

        if (scheduled.Any(m =>
                IsTeamInMatch(
                    m,
                    match.Team1RegistrationId) &&
                m.SlotNumber == slot.SlotNumber - 1))
        {
            penalty += 100;
        }

        if (scheduled.Any(m =>
                IsTeamInMatch(
                    m,
                    match.Team2RegistrationId) &&
                m.SlotNumber == slot.SlotNumber - 1))
        {
            penalty += 100;
        }

        return penalty;
    }

    private static bool IsTeamInMatch(
        ScheduledMatch match,
        int registrationId)
    {
        return match.Team1RegistrationId == registrationId ||
               match.Team2RegistrationId == registrationId;
    }

    private static int GetEarliestRequiredSlot(
        GeneratedMatch match)
    {
        return Math.Max(
            match.Team1EarliestMatchSlot,
            match.Team2EarliestMatchSlot);
    }

    private static bool IsCourtAvailable(
        Court court,
        DateTime startTime,
        DateTime endTime)
    {
        var start = startTime.TimeOfDay;
        var end = endTime.TimeOfDay;

        return court.Availabilities.Any(a =>
            start >= a.StartTime &&
            end <= a.EndTime);
    }

    private static List<TimeSlot> BuildSlots(
        DateTime start,
        DateTime end)
    {
        var slots = new List<TimeSlot>();

        var current = start;
        var slotNumber = 1;

        while (current.AddMinutes(SlotDurationMinutes) <= end)
        {
            slots.Add(new TimeSlot
            {
                SlotNumber = slotNumber,
                StartTime = current,
                EndTime = current.AddMinutes(
                    SlotDurationMinutes)
            });

            current = current.AddMinutes(
                SlotDurationMinutes);

            slotNumber++;
        }

        return slots;
    }

    private static List<GeneratedMatch> GenerateRoundRobinMatches(
        List<Pool> pools)
    {
        var matches = new List<GeneratedMatch>();

        foreach (var pool in pools.OrderBy(p => p.SortOrder))
        {
            var teams = pool.Entries
                .OrderBy(e => e.Position)
                .Select(e => new
                {
                    e.RegistrationId,
                    EarliestMatchSlot =
                        e.Registration.EarliestMatchSlot ?? 1
                })
                .ToList();

            for (var i = 0; i < teams.Count; i++)
            {
                for (var j = i + 1; j < teams.Count; j++)
                {
                    matches.Add(new GeneratedMatch
                    {
                        PoolId = pool.Id,
                        Team1RegistrationId =
                            teams[i].RegistrationId,
                        Team2RegistrationId =
                            teams[j].RegistrationId,
                        Team1EarliestMatchSlot =
                            teams[i].EarliestMatchSlot,
                        Team2EarliestMatchSlot =
                            teams[j].EarliestMatchSlot
                    });
                }
            }
        }

        return matches;
    }

    private class GeneratedMatch
    {
        public int PoolId { get; set; }

        public int Team1RegistrationId { get; set; }

        public int Team2RegistrationId { get; set; }

        public int Team1EarliestMatchSlot { get; set; }

        public int Team2EarliestMatchSlot { get; set; }
    }

    private class TimeSlot
    {
        public int SlotNumber { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    private class Candidate
    {
        public int CourtId { get; set; }

        public int SlotNumber { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Penalty { get; set; }
    }

    private readonly record struct CourtSlotKey(
        int CourtId,
        DateTime StartTime);

    private readonly record struct TeamSlotKey(
        int RegistrationId,
        DateTime StartTime);
}