using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public EventsController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
    {
        var events = await _context.Events
            .Where(e => e.IsActive)
            .Include(e => e.Tournaments)
            .ThenInclude(t => t.Category)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Date = e.Date,
                Location = e.Location,
                Address = e.Address,
                PeriodLabel = e.PeriodLabel,

                Tournaments = e.Tournaments
                    .Where(e => e.IsActive)
                    .OrderBy(t => t.SortOrder)
                    .Select(t => new TournamentDto
                    {
                        Id = t.Id,
                        SortOrder = t.SortOrder,
                        TabLabel = t.TabLabel,
                        Title = t.Title,
                        Format = t.Format,
                        StartTime = t.StartTime,
                        EndTime = t.EndTime,
                        Price = t.Price,
                        MaxTeams = t.MaxTeams,
                        Level = t.Level,
                        MinPlayers = t.MinPlayers,
                        FormUrl = t.FormUrl,
                        WaitlistFormUrl = t.WaitlistFormUrl,

                        Category = new CategoryDto
                        {
                            Id = t.Category.Id,
                            Name = t.Category.Name
                        }
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var eventDto = await _context.Events
            .Where(e => e.Id == id && e.IsActive)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Date = e.Date,
                Location = e.Location,
                Address = e.Address,
                PeriodLabel = e.PeriodLabel,

                Tournaments = e.Tournaments
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.SortOrder)
                    .Select(t => new TournamentDto
                    {
                        Id = t.Id,
                        SortOrder = t.SortOrder,
                        TabLabel = t.TabLabel,
                        Title = t.Title,
                        Format = t.Format,
                        StartTime = t.StartTime,
                        EndTime = t.EndTime,
                        Price = t.Price,
                        MaxTeams = t.MaxTeams,
                        Level = t.Level,
                        MinPlayers = t.MinPlayers,
                        FormUrl = t.FormUrl,
                        WaitlistFormUrl = t.WaitlistFormUrl,

                        Category = new CategoryDto
                        {
                            Id = t.Category.Id,
                            Name = t.Category.Name
                        }
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (eventDto == null)
        {
            return NotFound();
        }

        return Ok(eventDto);
    }
}