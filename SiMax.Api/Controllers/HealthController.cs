using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;

namespace SiMaxBE.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly SiMaxDbContext _context;

    public HealthController(SiMaxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok"
        });
    }

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        await _context.Database.ExecuteSqlRawAsync("SELECT 1");

        return Ok(new
        {
            status = "ok",
            database = "ok"
        });
    }
}