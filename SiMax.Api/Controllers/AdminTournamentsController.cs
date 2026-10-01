using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/tournaments")]
[Authorize(Policy = "AdminOnly")]
public class AdminTournamentsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminTournamentsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminTournamentDto>>> GetTournaments()
    {
        var tournaments = await _context.Tournaments
            .OrderBy(t => t.EventId)
            .ThenBy(t => t.SortOrder)
            .Select(t => new AdminTournamentDto
            {
                Id = t.Id,
                EventId = t.EventId,
                CategoryId = t.CategoryId,
                SortOrder = t.SortOrder,
                TabLabel = t.TabLabel,
                Title = t.Title,
                Format = t.Format,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                Price = t.Price,
                MaxTeams = t.MaxTeams,
                Level = t.Level,
                MinPlayers = t.MinPlayers,
                FormUrl = t.FormUrl,
                WaitlistFormUrl = t.WaitlistFormUrl,
                GoogleFormId = t.GoogleFormId,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(tournaments);
    }

    [HttpPost]
    public async Task<ActionResult<AdminTournamentDto>> CreateTournament(
    AdminTournamentRequest request)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == request.EventId);

        if (!eventExists)
        {
            return BadRequest("Evento non trovato.");
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Categoria non trovata.");
        }

        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(
                "L'orario di fine deve essere successivo all'orario di inizio.");
        }

        var tournament = new Tournament
        {
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            SortOrder = request.SortOrder,
            TabLabel = request.TabLabel.Trim(),
            Title = request.Title.Trim(),
            Format = request.Format.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Price = request.Price,
            MaxTeams = request.MaxTeams,
            Level = request.Level.Trim(),
            MinPlayers = request.MinPlayers,
            FormUrl = request.FormUrl.Trim(),
            WaitlistFormUrl = request.WaitlistFormUrl.Trim(),
            GoogleFormId = request.GoogleFormId.Trim(),
            IsActive = request.IsActive
        };

        _context.Tournaments.Add(tournament);

        await _context.SaveChangesAsync();

        var result = new AdminTournamentDto
        {
            Id = tournament.Id,
            EventId = tournament.EventId,
            CategoryId = tournament.CategoryId,
            SortOrder = tournament.SortOrder,
            TabLabel = tournament.TabLabel,
            Title = tournament.Title,
            Format = tournament.Format,
            StartTime = tournament.StartTime,
            EndTime = tournament.EndTime,
            Price = tournament.Price,
            MaxTeams = tournament.MaxTeams,
            Level = tournament.Level,
            MinPlayers = tournament.MinPlayers,
            FormUrl = tournament.FormUrl,
            WaitlistFormUrl = tournament.WaitlistFormUrl,
            GoogleFormId = tournament.GoogleFormId,
            IsActive = tournament.IsActive
        };

        return CreatedAtAction(
            nameof(GetTournaments),
            new { id = tournament.Id },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminTournamentDto>> UpdateTournament(
    int id,
    AdminTournamentRequest request)
    {
        var tournament = await _context.Tournaments
            .FindAsync(id);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == request.EventId);

        if (!eventExists)
        {
            return BadRequest("Evento non trovato.");
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Categoria non trovata.");
        }

        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(
                "L'orario di fine deve essere successivo all'orario di inizio.");
        }

        tournament.EventId = request.EventId;
        tournament.CategoryId = request.CategoryId;
        tournament.SortOrder = request.SortOrder;
        tournament.TabLabel = request.TabLabel.Trim();
        tournament.Title = request.Title.Trim();
        tournament.Format = request.Format.Trim();
        tournament.StartTime = request.StartTime;
        tournament.EndTime = request.EndTime;
        tournament.Price = request.Price;
        tournament.MaxTeams = request.MaxTeams;
        tournament.Level = request.Level.Trim();
        tournament.MinPlayers = request.MinPlayers;
        tournament.FormUrl = request.FormUrl.Trim();
        tournament.WaitlistFormUrl = request.WaitlistFormUrl.Trim();
        tournament.GoogleFormId = request.GoogleFormId.Trim();
        tournament.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        var result = new AdminTournamentDto
        {
            Id = tournament.Id,
            EventId = tournament.EventId,
            CategoryId = tournament.CategoryId,
            SortOrder = tournament.SortOrder,
            TabLabel = tournament.TabLabel,
            Title = tournament.Title,
            Format = tournament.Format,
            StartTime = tournament.StartTime,
            EndTime = tournament.EndTime,
            Price = tournament.Price,
            MaxTeams = tournament.MaxTeams,
            Level = tournament.Level,
            MinPlayers = tournament.MinPlayers,
            FormUrl = tournament.FormUrl,
            WaitlistFormUrl = tournament.WaitlistFormUrl,
            GoogleFormId = tournament.GoogleFormId,
            IsActive = tournament.IsActive
        };

        return Ok(result);
    }

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetTournamentActive(
    int id,
    [FromBody] bool isActive)
    {
        var tournament = await _context.Tournaments
            .FindAsync(id);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        tournament.IsActive = isActive;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = tournament.Id,
            isActive = tournament.IsActive
        });
    }

    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<AdminTournamentDto>> Duplicate(int id)
    {
        var sourceTournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == id);

        if (sourceTournament == null)
        {
            return NotFound();
        }

        var maxSortOrder = await _context.Tournaments
                                            .Where(t => t.EventId == sourceTournament.EventId)
                                            .Select(t => (int?)t.SortOrder)
                                            .MaxAsync() ?? 0;

        var newTournament = new Tournament
        {
            EventId = sourceTournament.EventId,
            CategoryId = sourceTournament.CategoryId,
            SortOrder = maxSortOrder + 1,
            TabLabel = sourceTournament.TabLabel,
            Title = $"{sourceTournament.Title} - Copia",
            Format = sourceTournament.Format,
            StartTime = sourceTournament.StartTime,
            EndTime = sourceTournament.EndTime,
            Price = sourceTournament.Price,
            MaxTeams = sourceTournament.MaxTeams,
            Level = sourceTournament.Level,
            MinPlayers = sourceTournament.MinPlayers,

            FormUrl = string.Empty,
            WaitlistFormUrl = string.Empty,
            GoogleFormId = string.Empty,

            IsActive = false
        };

        _context.Tournaments.Add(newTournament);

        await _context.SaveChangesAsync();

        return Ok(new AdminTournamentDto
        {
            Id = newTournament.Id,
            EventId = newTournament.EventId,
            CategoryId = newTournament.CategoryId,
            SortOrder = newTournament.SortOrder,
            TabLabel = newTournament.TabLabel,
            Title = newTournament.Title,
            Format = newTournament.Format,
            StartTime = newTournament.StartTime,
            EndTime = newTournament.EndTime,
            Price = newTournament.Price,
            MaxTeams = newTournament.MaxTeams,
            Level = newTournament.Level,
            MinPlayers = newTournament.MinPlayers,
            FormUrl = newTournament.FormUrl,
            WaitlistFormUrl = newTournament.WaitlistFormUrl,
            GoogleFormId = newTournament.GoogleFormId,
            IsActive = newTournament.IsActive
        });
    }
}