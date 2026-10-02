using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/tournaments/{tournamentId}/pools")]
[Authorize(Policy = "AdminOnly")]
public class AdminPoolsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminPoolsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPools(int tournamentId)
    {
        var tournamentExists = await _context.Tournaments
            .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
        {
            return NotFound("Torneo non trovato.");
        }

        var pools = await _context.Pools
            .Where(p => p.TournamentId == tournamentId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Id,
                p.TournamentId,
                p.Name,
                p.SortOrder,
                Entries = p.Entries
                    .OrderBy(entry => entry.Position)
                    .Select(entry => new
                    {
                        entry.Id,
                        entry.RegistrationId,
                        entry.Position,
                        TeamName = entry.Registration.TeamName,
                        entry.Registration.Player1FirstName,
                        entry.Registration.Player1LastName,
                        entry.Registration.Player2FirstName,
                        entry.Registration.Player2LastName
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(pools);
    }

    [HttpPost("preview")]
    public async Task<IActionResult> PreviewPools(
    int tournamentId,
    GeneratePoolsRequest request)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        var confirmedRegistrations = await _context.Registrations
            .Where(r =>
                r.TournamentId == tournamentId &&
                r.Status == "Confirmed")
            .OrderBy(r => r.Id)
            .ToListAsync();

        var confirmedTeams = confirmedRegistrations.Count;

        if (confirmedTeams == 0)
        {
            return BadRequest(
                "Non ci sono squadre confermate per questo torneo.");
        }

        var totalCapacity =
            request.PoolCount * request.MaxTeamsPerPool;

        if (totalCapacity < confirmedTeams)
        {
            return BadRequest(
                $"La capacità totale dei gironi ({totalCapacity}) " +
                $"è inferiore alle squadre confermate ({confirmedTeams}).");
        }

        if (request.Criterion != "Balanced" &&
            request.Criterion != "Random" &&
            request.Criterion != "Alphabetical")
        {
            return BadRequest(
                "Criterio non valido. Valori consentiti: " +
                "Balanced, Random, Alphabetical.");
        }

        List<Registration> registrationsToDistribute;

        switch (request.Criterion)
        {
            case "Alphabetical":
                registrationsToDistribute = confirmedRegistrations
                    .OrderBy(r => r.TeamName)
                    .ThenBy(r => r.Id)
                    .ToList();
                break;

            case "Random":
                registrationsToDistribute = confirmedRegistrations
                    .OrderBy(_ => Random.Shared.Next())
                    .ToList();
                break;

            case "Balanced":
                registrationsToDistribute = confirmedRegistrations
                    .OrderBy(r => r.Id)
                    .ToList();
                break;

            default:
                return BadRequest(
                    "Criterio non valido. Valori consentiti: " +
                    "Balanced, Random, Alphabetical.");
        }

        var pools = Enumerable
            .Range(0, request.PoolCount)
            .Select(i => new PoolPreviewDto
            {
                Name = $"Girone {(char)('A' + i)}",
                SortOrder = i + 1
            })
            .ToList();

        for (var i = 0; i < registrationsToDistribute.Count; i++)
        {
            var poolIndex = i % request.PoolCount;

            var registration = registrationsToDistribute[i];

            if (pools[poolIndex].Teams.Count >= request.MaxTeamsPerPool)
            {
                return BadRequest(
                    $"Il Girone {(char)('A' + poolIndex)} " +
                    "ha raggiunto il numero massimo di squadre.");
            }

            pools[poolIndex].Teams.Add(new PoolPreviewEntryDto
            {
                RegistrationId = registration.Id,
                TeamName = registration.TeamName,
                Player1FirstName = registration.Player1FirstName,
                Player1LastName = registration.Player1LastName,
                Player2FirstName = registration.Player2FirstName,
                Player2LastName = registration.Player2LastName
            });
        }

        return Ok(new
        {
            tournamentId,
            confirmedTeams,
            poolCount = request.PoolCount,
            maxTeamsPerPool = request.MaxTeamsPerPool,
            criterion = request.Criterion,
            pools
        });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPools(
    int tournamentId,
    ConfirmPoolsRequest request)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        if (request.Pools == null || request.Pools.Count == 0)
        {
            return BadRequest("Deve essere presente almeno un girone.");
        }

        var confirmedRegistrations = await _context.Registrations
            .Where(r =>
                r.TournamentId == tournamentId &&
                r.Status == "Confirmed")
            .ToListAsync();

        var confirmedRegistrationIds = confirmedRegistrations
            .Select(r => r.Id)
            .ToHashSet();

        var requestedRegistrationIds = request.Pools
            .SelectMany(p => p.RegistrationIds)
            .ToList();

        // Controlla che non ci siano duplicati
        if (requestedRegistrationIds.Count !=
            requestedRegistrationIds.Distinct().Count())
        {
            return BadRequest(
                "Una o più squadre sono presenti in più gironi.");
        }

        // Controlla che tutte le squadre appartengano
        // al torneo e siano confermate
        var invalidRegistrationIds = requestedRegistrationIds
            .Where(id => !confirmedRegistrationIds.Contains(id))
            .Distinct()
            .ToList();

        if (invalidRegistrationIds.Count > 0)
        {
            return BadRequest(
                "Una o più squadre non appartengono al torneo " +
                "oppure non sono confermate.");
        }

        // Controlla che tutte le squadre confermate
        // siano state inserite in un girone
        var missingRegistrationIds = confirmedRegistrationIds
            .Where(id => !requestedRegistrationIds.Contains(id))
            .ToList();

        if (missingRegistrationIds.Count > 0)
        {
            return BadRequest(
                "Non tutte le squadre confermate sono state assegnate " +
                "a un girone.");
        }

        // Controlla che non ci siano gironi duplicati
        var poolNames = request.Pools
            .Select(p => p.Name.Trim().ToLower())
            .ToList();

        if (poolNames.Count != poolNames.Distinct().Count())
        {
            return BadRequest("Sono presenti gironi con lo stesso nome.");
        }

        // Elimina eventuali gironi precedenti
        var existingPools = await _context.Pools
            .Where(p => p.TournamentId == tournamentId)
            .ToListAsync();

        _context.Pools.RemoveRange(existingPools);

        // Crea i nuovi gironi
        foreach (var poolRequest in request.Pools
                     .OrderBy(p => p.SortOrder))
        {
            var pool = new Pool
            {
                TournamentId = tournamentId,
                Name = poolRequest.Name.Trim(),
                SortOrder = poolRequest.SortOrder
            };

            for (var i = 0; i < poolRequest.RegistrationIds.Count; i++)
            {
                pool.Entries.Add(new PoolEntry
                {
                    RegistrationId = poolRequest.RegistrationIds[i],
                    Position = i + 1
                });
            }

            _context.Pools.Add(pool);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            tournamentId,
            poolsCreated = request.Pools.Count,
            teamsAssigned = requestedRegistrationIds.Count
        });
    }
}