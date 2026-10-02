using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.Models;
using SiMax.Api.DTOs;

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
    RegistrationRequest request,
    [FromHeader(Name = "X-Api-Key")] string? apiKey)
    {
        var expectedApiKey = _configuration["ApiKeys:Registrations"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != expectedApiKey)
        {
            return Unauthorized();
        }

        var tournament = await _context.Tournaments
    .FindAsync(request.TournamentId);

        if (tournament == null)
        {
            return BadRequest("Tournament not found.");
        }

        if (request.Status != "Confirmed" && request.Status != "Waitlist")
        {
            return BadRequest("Invalid registration status.");
        }

        var registration = new Registration
        {
            TournamentId = request.TournamentId,
            Email = request.Email.Trim(),
            TeamName = request.TeamName.Trim(),

            Player1FirstName = request.Player1FirstName.Trim(),
            Player1LastName = request.Player1LastName.Trim(),
            Player1Phone = request.Player1Phone.Trim(),

            Player2FirstName = request.Player2FirstName.Trim(),
            Player2LastName = request.Player2LastName.Trim(),

            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Registrations.Add(registration);

        await _context.SaveChangesAsync();

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetRegistration),
            new { id = registration.Id },
            registration);
    }

    [Authorize(Policy = "AdminOnly")]
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