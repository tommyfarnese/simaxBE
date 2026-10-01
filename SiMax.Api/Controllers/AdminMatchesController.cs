using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;
using SiMax.Api.Services.Scheduling;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/tournaments/{tournamentId}/matches")]
[Authorize(Policy = "AdminOnly")]
public class AdminMatchesController : ControllerBase
{
    private readonly SiMaxDbContext _context;
    private readonly MatchScheduler _scheduler;

    public AdminMatchesController(
        SiMaxDbContext context,
        MatchScheduler scheduler)
    {
        _context = context;
        _scheduler = scheduler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateMatches(
        int tournamentId)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound(new
            {
                message = "Torneo non trovato."
            });
        }

        var pools = await _context.Pools
            .Where(p => p.TournamentId == tournamentId)
            .Include(p => p.Entries)
                .ThenInclude(e => e.Registration)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        if (pools.Count == 0)
        {
            return BadRequest(new
            {
                message = "Non ci sono gironi configurati per questo torneo."
            });
        }

        var courts = await _context.TournamentCourts
            .Where(tc => tc.TournamentId == tournamentId)
            .Select(tc => tc.Court)
            .Include(c => c.Availabilities)
            .Where(c => c.IsActive)
            .ToListAsync();

        if (courts.Count == 0)
        {
            return BadRequest(new
            {
                message = "Non ci sono campi configurati per questo torneo."
            });
        }

        var existingMatches = await _context.Matches
            .Where(m => m.Tournament.EventId == tournament.EventId)
            .ToListAsync();

        var result = _scheduler.Schedule(
            tournament,
            pools,
            courts,
            existingMatches);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.ErrorMessage
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var oldMatches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();

            if (oldMatches.Count > 0)
            {
                _context.Matches.RemoveRange(oldMatches);
            }

            var matches = result.Matches
                .Select(m => new Match
                {
                    TournamentId = tournamentId,
                    PoolId = m.PoolId,
                    CourtId = m.CourtId,
                    MatchNumber = 0,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Team1RegistrationId =
                        m.Team1RegistrationId,
                    Team2RegistrationId =
                        m.Team2RegistrationId,
                    Status = "Scheduled"
                })
                .OrderBy(m => m.StartTime)
                .ThenBy(m => m.CourtId)
                .ToList();

            for (var i = 0; i < matches.Count; i++)
            {
                matches[i].MatchNumber = i + 1;
            }

            await _context.Matches.AddRangeAsync(matches);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Partite generate correttamente.",
                count = matches.Count,
                matches
            });
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Errore durante il salvataggio delle partite."
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMatches(int tournamentId)
    {
        var tournamentExists = await _context.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
        {
            return NotFound(new
            {
                message = "Torneo non trovato."
            });
        }

        var matches = await _context.Matches
            .Where(m => m.TournamentId == tournamentId)
            .Include(m => m.Pool)
            .Include(m => m.Court)
            .Include(m => m.Team1)
            .Include(m => m.Team2)
            .OrderBy(m => m.StartTime)
            .ThenBy(m => m.CourtId)
            .Select(m => new MatchDto
            {
                Id = m.Id,
                MatchNumber = m.MatchNumber,

                PoolId = m.PoolId,
                PoolName = m.Pool.Name,

                CourtId = m.CourtId,
                CourtName = m.Court.Name,

                StartTime = m.StartTime,
                EndTime = m.EndTime,

                Team1RegistrationId = m.Team1RegistrationId,
                Team1Name = m.Team1.TeamName,

                Team2RegistrationId = m.Team2RegistrationId,
                Team2Name = m.Team2.TeamName,

                Status = m.Status,
                Team1Score = m.Team1Score,
                Team2Score = m.Team2Score
            })
            .ToListAsync();

        return Ok(matches);
    }

    [HttpPut("{matchId}/result")]
    public async Task<IActionResult> UpdateResult(
    int matchId,
    UpdateMatchResultRequest request)
    {
        var match = await _context.Matches
            .Include(m => m.Pool)
            .Include(m => m.Court)
            .Include(m => m.Team1)
            .Include(m => m.Team2)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null)
        {
            return NotFound(new
            {
                message = "Partita non trovata."
            });
        }

        if (match.Status == "Cancelled")
        {
            return BadRequest(new
            {
                message = "Non è possibile inserire il risultato di una partita annullata."
            });
        }

        match.Team1Score = request.Team1Score;
        match.Team2Score = request.Team2Score;
        match.Status = "Completed";

        await _context.SaveChangesAsync();

        return Ok(new MatchDto
        {
            Id = match.Id,
            MatchNumber = match.MatchNumber,

            PoolId = match.PoolId,
            PoolName = match.Pool.Name,

            CourtId = match.CourtId,
            CourtName = match.Court.Name,

            StartTime = match.StartTime,
            EndTime = match.EndTime,

            Team1RegistrationId = match.Team1RegistrationId,
            Team1Name = match.Team1.TeamName,

            Team2RegistrationId = match.Team2RegistrationId,
            Team2Name = match.Team2.TeamName,

            Status = match.Status,
            Team1Score = match.Team1Score,
            Team2Score = match.Team2Score
        });
    }

    [HttpGet("~/api/admin/tournaments/{tournamentId}/pools/{poolId}/standings")]
    public async Task<IActionResult> GetStandings(
    int tournamentId,
    int poolId)
    {
        var pool = await _context.Pools
            .Include(p => p.Entries)
                .ThenInclude(e => e.Registration)
            .FirstOrDefaultAsync(p =>
                p.Id == poolId &&
                p.TournamentId == tournamentId);

        if (pool == null)
        {
            return NotFound(new
            {
                message = "Girone non trovato."
            });
        }

        var registrationIds = pool.Entries
            .Select(e => e.RegistrationId)
            .ToHashSet();

        var matches = await _context.Matches
            .Where(m =>
                m.TournamentId == tournamentId &&
                m.PoolId == poolId &&
                m.Status == "Completed" &&
                registrationIds.Contains(m.Team1RegistrationId) &&
                registrationIds.Contains(m.Team2RegistrationId))
            .ToListAsync();

        var standings = pool.Entries
            .Select(entry => new StandingDto
            {
                RegistrationId = entry.RegistrationId,
                TeamName = entry.Registration.TeamName
            })
            .ToList();

        foreach (var standing in standings)
        {
            var teamMatches = matches
                .Where(m =>
                    m.Team1RegistrationId == standing.RegistrationId ||
                    m.Team2RegistrationId == standing.RegistrationId)
                .ToList();

            foreach (var match in teamMatches)
            {
                if (!match.Team1Score.HasValue ||
                    !match.Team2Score.HasValue)
                {
                    continue;
                }

                standing.MatchesPlayed++;

                if (match.Team1RegistrationId == standing.RegistrationId)
                {
                    standing.PointsFor += match.Team1Score.Value;
                    standing.PointsAgainst += match.Team2Score.Value;

                    if (match.Team1Score.Value > match.Team2Score.Value)
                    {
                        standing.Wins++;
                    }
                    else
                    {
                        standing.Losses++;
                    }
                }
                else
                {
                    standing.PointsFor += match.Team2Score.Value;
                    standing.PointsAgainst += match.Team1Score.Value;

                    if (match.Team2Score.Value > match.Team1Score.Value)
                    {
                        standing.Wins++;
                    }
                    else
                    {
                        standing.Losses++;
                    }
                }
            }

            standing.PointDifference =
                standing.PointsFor - standing.PointsAgainst;
        }

        standings = standings
            .OrderByDescending(s => s.Wins)
            .ThenByDescending(s => s.PointsFor)
            .ThenByDescending(s => s.PointDifference)
            .ThenBy(s => s.TeamName)
            .ToList();

        for (var i = 0; i < standings.Count; i++)
        {
            standings[i].Position = i + 1;
        }

        return Ok(standings);
    }

    [HttpGet("~/api/admin/tournaments/{tournamentId}/standings")]
    public async Task<IActionResult> GetTournamentStandings(
    int tournamentId)
    {
        var tournamentExists = await _context.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
        {
            return NotFound(new
            {
                message = "Torneo non trovato."
            });
        }

        var pools = await _context.Pools
            .Where(p => p.TournamentId == tournamentId)
            .Include(p => p.Entries)
                .ThenInclude(e => e.Registration)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        var completedMatches = await _context.Matches
            .Where(m =>
                m.TournamentId == tournamentId &&
                m.Status == "Completed" &&
                m.Team1Score.HasValue &&
                m.Team2Score.HasValue)
            .ToListAsync();

        var result = new List<TournamentStandingsDto>();

        foreach (var pool in pools)
        {
            var registrationIds = pool.Entries
                .Select(e => e.RegistrationId)
                .ToHashSet();

            var poolMatches = completedMatches
                .Where(m =>
                    m.PoolId == pool.Id &&
                    registrationIds.Contains(m.Team1RegistrationId) &&
                    registrationIds.Contains(m.Team2RegistrationId))
                .ToList();

            var standings = pool.Entries
                .Select(entry => new StandingDto
                {
                    RegistrationId = entry.RegistrationId,
                    TeamName = entry.Registration.TeamName
                })
                .ToList();

            foreach (var standing in standings)
            {
                var teamMatches = poolMatches
                    .Where(m =>
                        m.Team1RegistrationId == standing.RegistrationId ||
                        m.Team2RegistrationId == standing.RegistrationId)
                    .ToList();

                foreach (var match in teamMatches)
                {
                    var team1Score = match.Team1Score!.Value;
                    var team2Score = match.Team2Score!.Value;

                    standing.MatchesPlayed++;

                    if (match.Team1RegistrationId == standing.RegistrationId)
                    {
                        standing.PointsFor += team1Score;
                        standing.PointsAgainst += team2Score;

                        if (team1Score > team2Score)
                        {
                            standing.Wins++;
                        }
                        else
                        {
                            standing.Losses++;
                        }
                    }
                    else
                    {
                        standing.PointsFor += team2Score;
                        standing.PointsAgainst += team1Score;

                        if (team2Score > team1Score)
                        {
                            standing.Wins++;
                        }
                        else
                        {
                            standing.Losses++;
                        }
                    }
                }

                standing.PointDifference =
                    standing.PointsFor - standing.PointsAgainst;
            }

            standings = standings
                .OrderByDescending(s => s.Wins)
                .ThenByDescending(s => s.PointsFor)
                .ThenByDescending(s => s.PointDifference)
                .ThenBy(s => s.TeamName)
                .ToList();

            for (var i = 0; i < standings.Count; i++)
            {
                standings[i].Position = i + 1;
            }

            result.Add(new TournamentStandingsDto
            {
                PoolId = pool.Id,
                PoolName = pool.Name,
                Standings = standings
            });
        }

        return Ok(result);
    }
}