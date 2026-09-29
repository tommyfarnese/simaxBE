using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiMax.Api.Data;
using SiMax.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SiMax.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly SiMaxDbContext _context;

        public TournamentsController(SiMaxDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tournament>>> GetTournaments()
        {
            var tournaments = await _context.Tournaments.ToListAsync();

            return Ok(tournaments);
        }

        [HttpPost]
        public async Task<ActionResult<Tournament>> CreateTournament(Tournament tournament)
        {
            _context.Tournaments.Add(tournament);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTournaments),
                new { id = tournament.Id },
                tournament);
        }
    }
}
