using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs;
using SiMax.Api.Services;

namespace SiMax.Api.Services;

public class EventsJsonService
{
    private readonly SiMaxDbContext _context;
    private readonly GitHubService _gitHubService;

    public EventsJsonService(
        SiMaxDbContext context,
        GitHubService gitHubService)
    {
        _context = context;
        _gitHubService = gitHubService;
    }

    public async Task RefreshAsync()
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
            .OrderBy(e => e.Date)
            .ToListAsync();

        var json = JsonSerializer.Serialize(
            new { events },
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await _gitHubService.UpdateFileAsync(
            json,
            "Update events.json");
    }
}