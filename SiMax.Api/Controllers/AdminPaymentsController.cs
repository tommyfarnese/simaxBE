using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/registrations/{registrationId}/payments")]
[Authorize(Policy = "AdminOnly")]
public class AdminPaymentsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public AdminPaymentsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<AdminPaymentDto>> Create(
    int registrationId,
    AdminPaymentRequest request)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.Id == registrationId);

        if (registration == null)
        {
            return NotFound("Registrazione non trovata.");
        }

        var allowedPaymentMethods = new[]
        {
        "PayPal",
        "Revolut",
        "Bonifico",
        "Satispay",
        "Contanti"
    };

        if (!allowedPaymentMethods.Contains(request.PaymentMethod))
        {
            return BadRequest(
                "Metodo di pagamento non valido. " +
                "Valori consentiti: PayPal, Revolut, Bonifico, Satispay, Contanti.");
        }

        if (request.PaymentMethod != "Contanti" &&
            string.IsNullOrWhiteSpace(request.ReceivedBy))
        {
            return BadRequest(
                "Per questo metodo di pagamento è necessario indicare chi ha ricevuto il pagamento.");
        }

        var payment = new Payment
        {
            RegistrationId = registrationId,
            PaymentMethod = request.PaymentMethod.Trim(),
            ReceivedBy = request.ReceivedBy.Trim(),
            Amount = request.Amount,
            PlayerNumber = request.PlayerNumber
        };

        _context.Set<Payment>().Add(payment);

        await _context.SaveChangesAsync();

        return Ok(new AdminPaymentDto
        {
            Id = payment.Id,
            RegistrationId = payment.RegistrationId,
            PaymentMethod = payment.PaymentMethod,
            ReceivedBy = payment.ReceivedBy,
            Amount = payment.Amount,
            PlayerNumber = payment.PlayerNumber
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminPaymentDto>>> GetAll(
    int registrationId)
    {
        var registrationExists = await _context.Registrations
            .AnyAsync(r => r.Id == registrationId);

        if (!registrationExists)
        {
            return NotFound("Registrazione non trovata.");
        }

        var payments = await _context.Set<Payment>()
            .Where(p => p.RegistrationId == registrationId)
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
            .ToListAsync();

        return Ok(payments);
    }

    [HttpPut("{paymentId}")]
    public async Task<ActionResult<AdminPaymentDto>> Update(
    int registrationId,
    int paymentId,
    AdminPaymentRequest request)
    {
        var payment = await _context.Set<Payment>()
            .FirstOrDefaultAsync(p =>
                p.Id == paymentId &&
                p.RegistrationId == registrationId);

        if (payment == null)
        {
            return NotFound("Pagamento non trovato.");
        }

        var allowedPaymentMethods = new[]
        {
        "PayPal",
        "Revolut",
        "Bonifico",
        "Satispay",
        "Contanti"
    };

        if (!allowedPaymentMethods.Contains(request.PaymentMethod))
        {
            return BadRequest(
                "Metodo di pagamento non valido. " +
                "Valori consentiti: PayPal, Revolut, Bonifico, Satispay, Contanti.");
        }

        if (request.PaymentMethod != "Contanti" &&
            string.IsNullOrWhiteSpace(request.ReceivedBy))
        {
            return BadRequest(
                "Per questo metodo di pagamento è necessario indicare chi ha ricevuto il pagamento.");
        }

        payment.PaymentMethod = request.PaymentMethod.Trim();
        payment.ReceivedBy = request.ReceivedBy.Trim();
        payment.Amount = request.Amount;
        payment.PlayerNumber = request.PlayerNumber;

        await _context.SaveChangesAsync();

        return Ok(new AdminPaymentDto
        {
            Id = payment.Id,
            RegistrationId = payment.RegistrationId,
            PaymentMethod = payment.PaymentMethod,
            ReceivedBy = payment.ReceivedBy,
            Amount = payment.Amount,
            PlayerNumber = payment.PlayerNumber
        });
    }

    [HttpDelete("{paymentId}")]
    public async Task<IActionResult> Delete(
        int registrationId,
        int paymentId)
    {
        var payment = await _context.Set<Payment>()
            .FirstOrDefaultAsync(p =>
                p.Id == paymentId &&
                p.RegistrationId == registrationId);

        if (payment == null)
        {
            return NotFound("Pagamento non trovato.");
        }

        _context.Set<Payment>().Remove(payment);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Pagamento eliminato."
        });
    }
}
