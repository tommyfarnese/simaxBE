using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminDashboardController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<AdminDashboardDto>> Get()
    {
        var events = await _context.Events
            .Include(e => e.Tournaments)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var tournamentIds = events
            .SelectMany(e => e.Tournaments)
            .Select(t => t.Id)
            .ToList();

        var registrations = await _context.Registrations
            .Where(r => tournamentIds.Contains(r.TournamentId))
            .ToListAsync();

        var result = new AdminDashboardDto();

        foreach (var ev in events)
        {
            var eventDto = new AdminDashboardEventDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Date = ev.Date,
                IsActive = ev.IsActive
            };

            foreach (var tournament in ev.Tournaments
                         .OrderBy(t => t.SortOrder))
            {
                var tournamentRegistrations = registrations
                    .Where(r => r.TournamentId == tournament.Id);

                var confirmed = tournamentRegistrations
                    .Count(r => r.Status == "Confirmed");

                var waitlist = tournamentRegistrations
                    .Count(r => r.Status == "Waitlist");

                var cancelled = tournamentRegistrations
                    .Count(r => r.Status == "Cancelled");

                eventDto.Tournaments.Add(new AdminDashboardTournamentDto
                {
                    Id = tournament.Id,
                    Title = tournament.Title,
                    MaxTeams = tournament.MaxTeams,
                    IsActive = tournament.IsActive,
                    ConfirmedCount = confirmed,
                    WaitlistCount = waitlist,
                    CancelledCount = cancelled,
                    AvailableSlots = Math.Max(
                        0,
                        tournament.MaxTeams - confirmed)
                });
            }

            result.Events.Add(eventDto);
        }

        return Ok(result);
    }


    [HttpGet("/api/admin/events/{eventId}/participants/prices")]
    public async Task<ActionResult<List<AdminParticipantPriceDto>>> GetParticipantPrices(
    int eventId)
    {
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return NotFound("Evento non trovato.");
        }

        var registrations = await _context.Registrations
            .Include(r => r.Tournament)
            .Include(r => r.Payments)
            .Where(r =>
                r.Tournament.EventId == eventId &&
                r.Status == "Confirmed")
            .ToListAsync();

        var participants = new List<(string FirstName, string LastName, int TournamentId, int RegistrationId, int PlayerNumber)>();

        foreach (var registration in registrations)
        {
            participants.Add((
                registration.Player1FirstName.Trim(),
                registration.Player1LastName.Trim(),
                registration.TournamentId,
                registration.Id,
                1));

            participants.Add((
                registration.Player2FirstName.Trim(),
                registration.Player2LastName.Trim(),
                registration.TournamentId,
                registration.Id,
                2));
        }

        var groupedParticipants = participants
            .GroupBy(p => new
            {
                FirstName = p.FirstName.ToLower(),
                LastName = p.LastName.ToLower()
            });

        var result = new List<AdminParticipantPriceDto>();

        foreach (var group in groupedParticipants)
        {
            var tournamentIds = group
                .Select(p => p.TournamentId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var tournamentCount = tournamentIds.Count;

            int amountDue;

            if (tournamentCount == 1)
            {
                amountDue = eventEntity.PriceOneTournament;
            }
            else if (tournamentCount == 2)
            {
                amountDue = eventEntity.PriceTwoTournaments;
            }
            else if (tournamentCount == 3)
            {
                amountDue = eventEntity.PriceThreeTournaments;
            }
            else
            {
                return BadRequest(
                    $"Il partecipante {group.First().FirstName} {group.First().LastName} " +
                    $"risulta iscritto a {tournamentCount} tornei.");
            }

            // Le tariffe per evento sono configurabili. Per gli eventi creati prima
            // dell'introduzione di questi campi, manteniamo la tariffa standard di
            // 20 € per torneo invece di mostrare un importo dovuto pari a zero.
            if (amountDue == 0)
            {
                amountDue = tournamentCount * 20;
            }

            var amountPaid = 0;

            foreach (var participant in group)
            {
                var registration = registrations
                    .First(r => r.Id == participant.RegistrationId);

                var playerPayments = registration.Payments
                    .Where(p => p.PlayerNumber == participant.PlayerNumber);

                amountPaid += playerPayments.Sum(p => p.Amount);
            }

            var amountRemaining = amountDue - amountPaid;

            var first = group.First();

            result.Add(new AdminParticipantPriceDto
            {
                FirstName = first.FirstName,
                LastName = first.LastName,
                TournamentCount = tournamentCount,
                AmountDue = amountDue,
                AmountPaid = amountPaid,
                AmountRemaining = amountRemaining,
                TournamentIds = tournamentIds
            });
        }

        result = result
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();

        return Ok(result);
    }

    [HttpGet("/api/admin/events/{eventId}/payments/summary")]
    public async Task<ActionResult<AdminPaymentSummaryDto>> GetPaymentSummary(
    int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId);

        if (!eventExists)
        {
            return NotFound("Evento non trovato.");
        }

        var payments = await _context.Set<Payment>()
            .Include(p => p.Registration)
            .ThenInclude(r => r.Tournament)
            .Where(p => p.Registration.Tournament.EventId == eventId)
            .ToListAsync();

        var totalAmount = payments.Sum(p => p.Amount);

        var nonCashPayments = payments
            .Where(p => p.PaymentMethod != "Contanti")
            .ToList();

        var massimoAmount = nonCashPayments
            .Where(p => p.ReceivedBy.Equals("Massimo", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Amount);

        var silviaAmount = nonCashPayments
            .Where(p => p.ReceivedBy.Equals("Silvia", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Amount);

        var difference = Math.Abs(massimoAmount - silviaAmount);
        var settlementAmount = difference / 2m;

        string settlement;

        if (settlementAmount == 0)
        {
            settlement = "Nessun conguaglio.";
        }
        else if (massimoAmount > silviaAmount)
        {
            settlement = $"Massimo deve dare {settlementAmount:0.00} € a Silvia.";
        }
        else
        {
            settlement = $"Silvia deve dare {settlementAmount:0.00} € a Massimo.";
        }

        return Ok(new AdminPaymentSummaryDto
        {
            TotalAmount = totalAmount,
            MassimoAmount = massimoAmount,
            SilviaAmount = silviaAmount,
            Difference = settlementAmount,
            Settlement = settlement
        });
    }
}
