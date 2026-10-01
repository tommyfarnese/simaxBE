using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/tournaments/{tournamentId}/courts")]
[Authorize(Policy = "AdminOnly")]
public class AdminTournamentCourtsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminTournamentCourtsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTournamentCourts(int tournamentId)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.TournamentCourts)
                .ThenInclude(tc => tc.Court)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        var courts = tournament.TournamentCourts
            .Where(tc => tc.Court.IsActive)
            .OrderBy(tc => tc.Court.SortOrder)
            .Select(tc => new
            {
                tc.Court.Id,
                tc.Court.Name,
                tc.Court.SortOrder,
                tc.Court.IsActive
            })
            .ToList();

        return Ok(courts);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTournamentCourts(
        int tournamentId,
        UpdateTournamentCourtsRequest request)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        var courtIds = request.CourtIds
            .Distinct()
            .ToList();

        var courts = await _context.Courts
            .Where(c =>
                c.EventId == tournament.EventId &&
                c.IsActive &&
                courtIds.Contains(c.Id))
            .ToListAsync();

        if (courts.Count != courtIds.Count)
        {
            return BadRequest(
                "Uno o più campi non appartengono all'evento del torneo, " +
                "non esistono oppure non sono attivi.");
        }

        var existing = await _context.TournamentCourts
            .Where(tc => tc.TournamentId == tournamentId)
            .ToListAsync();

        _context.TournamentCourts.RemoveRange(existing);

        foreach (var courtId in courtIds)
        {
            _context.TournamentCourts.Add(new Models.TournamentCourt
            {
                TournamentId = tournamentId,
                CourtId = courtId
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            tournamentId,
            courtIds
        });
    }
}