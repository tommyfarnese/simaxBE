using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Policy = "AdminOnly")]
public class AdminCategoriesController : ControllerBase
{
    private readonly SiMaxDbContext _db;

    public AdminCategoriesController(SiMaxDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminCategoryDto>>> GetAll()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<AdminCategoryDto>> Create(
        AdminCategoryRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _db.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return Conflict("Esiste già una categoria con questo nome.");
        }

        var category = new Category
        {
            Name = name
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var dto = new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name
        };

        return CreatedAtAction(
            nameof(GetAll),
            new { id = category.Id },
            dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminCategoryDto>> Update(
        int id,
        AdminCategoryRequest request)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();

        var exists = await _db.Categories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return Conflict("Esiste già una categoria con questo nome.");
        }

        category.Name = name;

        await _db.SaveChangesAsync();

        return Ok(new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name
        });
    }
}