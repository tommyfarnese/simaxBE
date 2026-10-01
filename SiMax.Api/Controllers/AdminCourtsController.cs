using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/events/{eventId}/courts")]
[Authorize(Policy = "AdminOnly")]
public class AdminCourtsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminCourtsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourts(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId);

        if (!eventExists)
        {
            return NotFound("Evento non trovato.");
        }

        var courts = await _context.Courts
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.EventId,
                c.Name,
                c.SortOrder,
                c.IsActive
            })
            .ToListAsync();

        return Ok(courts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourt(
        int eventId,
        CourtRequest request)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId);

        if (!eventExists)
        {
            return NotFound("Evento non trovato.");
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Il nome del campo è obbligatorio.");
        }

        var nameExists = await _context.Courts
            .AnyAsync(c =>
                c.EventId == eventId &&
                c.Name.ToLower() == name.ToLower());

        if (nameExists)
        {
            return BadRequest("Esiste già un campo con questo nome.");
        }

        var court = new Court
        {
            EventId = eventId,
            Name = name,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        _context.Courts.Add(court);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            court.Id,
            court.EventId,
            court.Name,
            court.SortOrder,
            court.IsActive
        });
    }

    [HttpPut("{courtId}")]
    public async Task<IActionResult> UpdateCourt(
        int eventId,
        int courtId,
        CourtRequest request)
    {
        var court = await _context.Courts
            .FirstOrDefaultAsync(c =>
                c.Id == courtId &&
                c.EventId == eventId);

        if (court == null)
        {
            return NotFound("Campo non trovato.");
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Il nome del campo è obbligatorio.");
        }

        var nameExists = await _context.Courts
            .AnyAsync(c =>
                c.EventId == eventId &&
                c.Id != courtId &&
                c.Name.ToLower() == name.ToLower());

        if (nameExists)
        {
            return BadRequest("Esiste già un campo con questo nome.");
        }

        court.Name = name;
        court.SortOrder = request.SortOrder;
        court.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            court.Id,
            court.EventId,
            court.Name,
            court.SortOrder,
            court.IsActive
        });
    }

    [HttpDelete("{courtId}")]
    public async Task<IActionResult> DeleteCourt(
        int eventId,
        int courtId)
    {
        var court = await _context.Courts
            .FirstOrDefaultAsync(c =>
                c.Id == courtId &&
                c.EventId == eventId);

        if (court == null)
        {
            return NotFound("Campo non trovato.");
        }

        court.IsActive = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Campo disattivato."
        });
    }
}