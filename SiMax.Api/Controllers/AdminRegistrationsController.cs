using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/registrations")]
[Authorize(Policy = "AdminOnly")]
public class AdminRegistrationsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminRegistrationsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<ActionResult<List<AdminRegistrationDto>>> GetByTournament(
    int tournamentId)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound("Torneo non trovato.");
        }

        var registrations = await _context.Registrations
            .Include(r => r.Payments)
            .Where(r => r.TournamentId == tournamentId)
            .OrderBy(r => r.Status)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync();

        var result = new List<AdminRegistrationDto>();

        foreach (var registration in registrations)
        {
            var amountDue = tournament.Event.PriceOneTournament;

            var amountPaid = registration.Payments
                .Sum(p => p.Amount);

            var amountRemaining = amountDue - amountPaid;

            result.Add(new AdminRegistrationDto
            {
                Id = registration.Id,
                TournamentId = registration.TournamentId,

                Email = registration.Email,
                TeamName = registration.TeamName,

                Player1FirstName = registration.Player1FirstName,
                Player1LastName = registration.Player1LastName,
                Player1Phone = registration.Player1Phone,

                Player2FirstName = registration.Player2FirstName,
                Player2LastName = registration.Player2LastName,

                Status = registration.Status,
                CreatedAt = registration.CreatedAt,
                EarliestMatchSlot = registration.EarliestMatchSlot,
                AmountDue = amountDue,
                AmountPaid = amountPaid,
                AmountRemaining = amountRemaining,
                Payments = registration.Payments
                    .OrderBy(p => p.Id)
                    .Select(p => new AdminPaymentDto
                    {
                        Id = p.Id,
                        RegistrationId = p.RegistrationId,
                        PlayerNumber = p.PlayerNumber,
                        PaymentMethod = p.PaymentMethod,
                        ReceivedBy = p.ReceivedBy,
                        Amount = p.Amount
                    })
                    .ToList()
            });
        }

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<AdminRegistrationDto>> UpdateStatus(
    int id,
    AdminRegistrationStatusRequest request)
    {
        var registration = await _context.Registrations
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id);

        if (registration == null)
        {
            return NotFound("Registrazione non trovata.");
        }

        var allowedStatuses = new[]
        {
        "Confirmed",
        "Waitlist",
        "Cancelled"
    };

        if (!allowedStatuses.Contains(request.Status))
        {
            return BadRequest(
                "Stato non valido. Valori consentiti: Confirmed, Waitlist, Cancelled.");
        }

        registration.Status = request.Status;

        await _context.SaveChangesAsync();

        var tournament = await _context.Tournaments
            .Include(t => t.Event)
            .FirstAsync(t => t.Id == registration.TournamentId);

        var amountDue = tournament.Event.PriceOneTournament;

        var amountPaid = await _context.Set<Payment>()
            .Where(p => p.RegistrationId == registration.Id)
            .SumAsync(p => p.Amount);

        var amountRemaining = amountDue - amountPaid;


        return Ok(new AdminRegistrationDto
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,

            Email = registration.Email,
            TeamName = registration.TeamName,

            Player1FirstName = registration.Player1FirstName,
            Player1LastName = registration.Player1LastName,
            Player1Phone = registration.Player1Phone,

            Player2FirstName = registration.Player2FirstName,
            Player2LastName = registration.Player2LastName,

            Status = registration.Status,
            CreatedAt = registration.CreatedAt,
            EarliestMatchSlot = registration.EarliestMatchSlot,
            AmountDue = amountDue,
            AmountPaid = amountPaid,
            AmountRemaining = amountRemaining,
            Payments = registration.Payments
                .OrderBy(p => p.Id)
                .Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    RegistrationId = p.RegistrationId,
                    PlayerNumber = p.PlayerNumber,
                    PaymentMethod = p.PaymentMethod,
                    ReceivedBy = p.ReceivedBy,
                    Amount = p.Amount
                })
                .ToList()
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminRegistrationDto>> Update(
    int id,
    AdminRegistrationUpdateRequest request)
    {
        var registration = await _context.Registrations
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id);

        if (registration == null)
        {
            return NotFound("Registrazione non trovata.");
        }

        registration.TeamName = request.TeamName.Trim();
        registration.Email = request.Email.Trim();

        registration.Player1FirstName = request.Player1FirstName.Trim();
        registration.Player1LastName = request.Player1LastName.Trim();
        registration.Player1Phone = request.Player1Phone.Trim();

        registration.Player2FirstName = request.Player2FirstName.Trim();
        registration.Player2LastName = request.Player2LastName.Trim();

        await _context.SaveChangesAsync();

        var tournament = await _context.Tournaments
            .Include(t => t.Event)
    .       FirstAsync(t => t.Id == registration.TournamentId);

        var amountDue = tournament.Event.PriceOneTournament;

        var amountPaid = await _context.Set<Payment>()
            .Where(p => p.RegistrationId == registration.Id)
            .SumAsync(p => p.Amount);

        var amountRemaining = amountDue - amountPaid;

        return Ok(new AdminRegistrationDto
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,

            Email = registration.Email,
            TeamName = registration.TeamName,

            Player1FirstName = registration.Player1FirstName,
            Player1LastName = registration.Player1LastName,
            Player1Phone = registration.Player1Phone,

            Player2FirstName = registration.Player2FirstName,
            Player2LastName = registration.Player2LastName,

            Status = registration.Status,
            CreatedAt = registration.CreatedAt,
            EarliestMatchSlot = registration.EarliestMatchSlot,
            AmountDue = amountDue,
            AmountPaid = amountPaid,
            AmountRemaining = amountRemaining,
            Payments = registration.Payments
                .OrderBy(p => p.Id)
                .Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    RegistrationId = p.RegistrationId,
                    PlayerNumber = p.PlayerNumber,
                    PaymentMethod = p.PaymentMethod,
                    ReceivedBy = p.ReceivedBy,
                    Amount = p.Amount
                })
                .ToList()
        });
    }
}