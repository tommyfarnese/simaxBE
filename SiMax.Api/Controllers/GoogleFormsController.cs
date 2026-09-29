using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/google-forms")]
public class GoogleFormsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public GoogleFormsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet("tournament")]
    public async Task<IActionResult> GetTournamentByFormId(
        [FromQuery] string formId)
    {
        if (string.IsNullOrWhiteSpace(formId))
        {
            return BadRequest("formId is required.");
        }

        var tournament = await _context.Tournaments
            .Where(t => t.GoogleFormId == formId && t.IsActive)
            .Select(t => new
            {
                tournamentId = t.Id
            })
            .FirstOrDefaultAsync();

        if (tournament == null)
        {
            return NotFound("Google Form not associated with any tournament.");
        }

        return Ok(tournament);
    }
}