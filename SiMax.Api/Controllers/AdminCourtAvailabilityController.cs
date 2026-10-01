using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/events/{eventId}/courts/{courtId}/availability")]
[Authorize(Policy = "AdminOnly")]
public class AdminCourtAvailabilityController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminCourtAvailabilityController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailability(
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

        var availability = await _context.CourtAvailabilities
            .Where(a => a.CourtId == courtId)
            .OrderBy(a => a.StartTime)
            .Select(a => new
            {
                a.Id,
                a.CourtId,
                a.StartTime,
                a.EndTime
            })
            .ToListAsync();

        return Ok(availability);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAvailability(
        int eventId,
        int courtId,
        CourtAvailabilityRequest request)
    {
        var court = await _context.Courts
            .FirstOrDefaultAsync(c =>
                c.Id == courtId &&
                c.EventId == eventId);

        if (court == null)
        {
            return NotFound("Campo non trovato.");
        }

        if (request.StartTime >= request.EndTime)
        {
            return BadRequest(
                "L'orario di inizio deve essere precedente all'orario di fine.");
        }

        var overlaps = await _context.CourtAvailabilities
            .AnyAsync(a =>
                a.CourtId == courtId &&
                request.StartTime < a.EndTime &&
                request.EndTime > a.StartTime);

        if (overlaps)
        {
            return BadRequest(
                "La disponibilità si sovrappone a una disponibilità esistente.");
        }

        var availability = new CourtAvailability
        {
            CourtId = courtId,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _context.CourtAvailabilities.Add(availability);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            availability.Id,
            availability.CourtId,
            availability.StartTime,
            availability.EndTime
        });
    }

    [HttpPut("{availabilityId}")]
    public async Task<IActionResult> UpdateAvailability(
        int eventId,
        int courtId,
        int availabilityId,
        CourtAvailabilityRequest request)
    {
        var courtExists = await _context.Courts
            .AnyAsync(c =>
                c.Id == courtId &&
                c.EventId == eventId);

        if (!courtExists)
        {
            return NotFound("Campo non trovato.");
        }

        var availability = await _context.CourtAvailabilities
            .FirstOrDefaultAsync(a =>
                a.Id == availabilityId &&
                a.CourtId == courtId);

        if (availability == null)
        {
            return NotFound("Disponibilità non trovata.");
        }

        if (request.StartTime >= request.EndTime)
        {
            return BadRequest(
                "L'orario di inizio deve essere precedente all'orario di fine.");
        }

        var overlaps = await _context.CourtAvailabilities
            .AnyAsync(a =>
                a.CourtId == courtId &&
                a.Id != availabilityId &&
                request.StartTime < a.EndTime &&
                request.EndTime > a.StartTime);

        if (overlaps)
        {
            return BadRequest(
                "La disponibilità si sovrappone a una disponibilità esistente.");
        }

        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            availability.Id,
            availability.CourtId,
            availability.StartTime,
            availability.EndTime
        });
    }

    [HttpDelete("{availabilityId}")]
    public async Task<IActionResult> DeleteAvailability(
        int eventId,
        int courtId,
        int availabilityId)
    {
        var courtExists = await _context.Courts
            .AnyAsync(c =>
                c.Id == courtId &&
                c.EventId == eventId);

        if (!courtExists)
        {
            return NotFound("Campo non trovato.");
        }

        var availability = await _context.CourtAvailabilities
            .FirstOrDefaultAsync(a =>
                a.Id == availabilityId &&
                a.CourtId == courtId);

        if (availability == null)
        {
            return NotFound("Disponibilità non trovata.");
        }

        _context.CourtAvailabilities.Remove(availability);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Disponibilità eliminata."
        });
    }
}