using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/events")]
[Authorize(Policy = "AdminOnly")]
public class AdminEventsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminEventsController(SiMaxDbContext context)
    {
        _context = context;
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
            IsActive = request.IsActive
        };

        _context.Events.Add(newEvent);

        await _context.SaveChangesAsync();

        var result = new AdminEventDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Date = newEvent.Date,
            Location = newEvent.Location,
            Address = newEvent.Address,
            PeriodLabel = newEvent.PeriodLabel,
            IsActive = newEvent.IsActive
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

        await _context.SaveChangesAsync();

        var result = new AdminEventDto
        {
            Id = existingEvent.Id,
            Title = existingEvent.Title,
            Date = existingEvent.Date,
            Location = existingEvent.Location,
            Address = existingEvent.Address,
            PeriodLabel = existingEvent.PeriodLabel,
            IsActive = existingEvent.IsActive
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

        return Ok(new
        {
            id = existingEvent.Id,
            isActive = existingEvent.IsActive
        });
    }
}