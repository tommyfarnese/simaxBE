using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly SiMaxDbContext _context;
    private readonly IConfiguration _configuration;

    public RegistrationsController(SiMaxDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<ActionResult<Registration>> CreateRegistration(
        Registration registration,
        [FromHeader(Name = "X-Api-Key")] string? apiKey)
    {
        var expectedApiKey = _configuration["ApiKeys:Registrations"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != expectedApiKey)
        {
            return Unauthorized();
        }

        var tournament = await _context.Tournaments
            .FindAsync(registration.TournamentId);

        if (tournament == null)
        {
            return BadRequest("Tournament not found.");
        }

        if (registration.Status != "Confirmed" && registration.Status != "Waitlist")
        {
            return BadRequest("Invalid registration status.");
        }

        registration.CreatedAt = DateTime.UtcNow;

        _context.Registrations.Add(registration);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetRegistration),
            new { id = registration.Id },
            registration);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Registration>> GetRegistration(int id)
    {
        var registration = await _context.Registrations
            .FindAsync(id);

        if (registration == null)
        {
            return NotFound();
        }

        return Ok(registration);
    }

    [HttpGet("tournament/{tournamentId}/status")]
    public async Task<IActionResult> GetTournamentStatus(int tournamentId)
    {
        var tournament = await _context.Tournaments
            .FindAsync(tournamentId);

        if (tournament == null)
        {
            return NotFound("Tournament not found.");
        }

        var confirmedRegistrations = await _context.Registrations
            .CountAsync(r =>
                r.TournamentId == tournamentId &&
                r.Status == "Confirmed");

        var waitlistRegistrations = await _context.Registrations
            .CountAsync(r =>
                r.TournamentId == tournamentId &&
                r.Status == "Waitlist");

        return Ok(new
        {
            tournamentId = tournamentId,
            confirmedRegistrations = confirmedRegistrations,
            waitlistRegistrations = waitlistRegistrations,
            maxTeams = tournament.MaxTeams
        });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRegistration(int id)
    {
        var registration = await _context.Registrations.FindAsync(id);

        if (registration == null)
        {
            return NotFound();
        }

        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}