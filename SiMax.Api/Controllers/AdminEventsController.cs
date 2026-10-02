using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;
using SiMax.Api.Services;
using System.Text.Json;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/events")]
[Authorize(Policy = "AdminOnly")]
public class AdminEventsController : ControllerBase
{
    private readonly SiMaxDbContext _context;
    private readonly EventsJsonService _eventsJsonService;

    public AdminEventsController(SiMaxDbContext context, EventsJsonService eventsJsonService)
    {
        _context = context;
        _eventsJsonService = eventsJsonService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminEventDto>>> GetEvents()
    {
        var events = await _context.Events
            .OrderBy(e => e.Date)
            .Select(e => new AdminEventDto
            {
                Id = e.Id,
                Title = e.Title,
                Date = e.Date,
                Location = e.Location,
                Address = e.Address,
                PeriodLabel = e.PeriodLabel,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpPost]
    public async Task<ActionResult<AdminEventDto>> CreateEvent(
    AdminEventRequest request)
    {
        var newEvent = new Event
        {
            Title = request.Title.Trim(),
            Date = request.Date,
            Location = request.Location.Trim(),
            Address = request.Address.Trim(),
            PeriodLabel = request.PeriodLabel.Trim(),
            IsActive = request.IsActive,
            PriceOneTournament = request.PriceOneTournament,
            PriceTwoTournaments = request.PriceTwoTournaments,
            PriceThreeTournaments = request.PriceThreeTournaments
        };

        _context.Events.Add(newEvent);

        await _context.SaveChangesAsync();

        await _eventsJsonService.RefreshAsync();

        var result = new AdminEventDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Date = newEvent.Date,
            Location = newEvent.Location,
            Address = newEvent.Address,
            PeriodLabel = newEvent.PeriodLabel,
            IsActive = newEvent.IsActive,
            PriceOneTournament = newEvent.PriceOneTournament,
            PriceTwoTournaments = newEvent.PriceTwoTournaments,
            PriceThreeTournaments = newEvent.PriceThreeTournaments
        };

        return CreatedAtAction(
            nameof(GetEvents),
            new { id = newEvent.Id },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminEventDto>> UpdateEvent(
    int id,
    AdminEventRequest request)
    {
        var existingEvent = await _context.Events
            .FindAsync(id);

        if (existingEvent == null)
        {
            return NotFound("Evento non trovato.");
        }

        existingEvent.Title = request.Title.Trim();
        existingEvent.Date = request.Date;
        existingEvent.Location = request.Location.Trim();
        existingEvent.Address = request.Address.Trim();
        existingEvent.PeriodLabel = request.PeriodLabel.Trim();
        existingEvent.IsActive = request.IsActive;
        existingEvent.PriceOneTournament = request.PriceOneTournament;
        existingEvent.PriceTwoTournaments = request.PriceTwoTournaments;
        existingEvent.PriceThreeTournaments = request.PriceThreeTournaments;

        await _context.SaveChangesAsync();

        await _eventsJsonService.RefreshAsync();

        var result = new AdminEventDto
        {
            Id = existingEvent.Id,
            Title = existingEvent.Title,
            Date = existingEvent.Date,
            Location = existingEvent.Location,
            Address = existingEvent.Address,
            PeriodLabel = existingEvent.PeriodLabel,
            IsActive = existingEvent.IsActive,
            PriceOneTournament = existingEvent.PriceOneTournament,
            PriceTwoTournaments = existingEvent.PriceTwoTournaments,
            PriceThreeTournaments = existingEvent.PriceThreeTournaments
        };

        return Ok(result);
    }

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetEventActive(
    int id,
    [FromBody] bool isActive)
    {
        var existingEvent = await _context.Events
            .FindAsync(id);

        if (existingEvent == null)
        {
            return NotFound("Evento non trovato.");
        }

        existingEvent.IsActive = isActive;

        await _context.SaveChangesAsync();

        await _eventsJsonService.RefreshAsync();

        return Ok(new
        {
            id = existingEvent.Id,
            isActive = existingEvent.IsActive
        });
    }

    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<AdminEventDto>> Duplicate(int id)
    {
        var sourceEvent = await _context.Events
            .Include(e => e.Tournaments)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (sourceEvent == null)
        {
            return NotFound();
        }

        var newEvent = new Event
        {
            Title = $"{sourceEvent.Title} - Copia",
            Date = sourceEvent.Date,
            Location = sourceEvent.Location,
            Address = sourceEvent.Address,
            PeriodLabel = sourceEvent.PeriodLabel,
            IsActive = false,
            PriceOneTournament = sourceEvent.PriceOneTournament,
            PriceTwoTournaments = sourceEvent.PriceTwoTournaments,
            PriceThreeTournaments = sourceEvent.PriceThreeTournaments
        };

        foreach (var tournament in sourceEvent.Tournaments)
        {
            newEvent.Tournaments.Add(new Tournament
            {
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

                // I nuovi tornei devono avere nuovi Google Form
                FormUrl = string.Empty,
                WaitlistFormUrl = string.Empty,
                GoogleFormId = string.Empty,

                IsActive = true
            });
        }

        _context.Events.Add(newEvent);

        await _context.SaveChangesAsync();

        await _eventsJsonService.RefreshAsync();

        return Ok(new AdminEventDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Date = newEvent.Date,
            Location = newEvent.Location,
            Address = newEvent.Address,
            PeriodLabel = newEvent.PeriodLabel,
            IsActive = newEvent.IsActive
        });
    }

    
}