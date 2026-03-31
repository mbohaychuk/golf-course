// MiquelonGolf.Api/Controllers/HolesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Holes;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/holes")]
public class HolesController : ControllerBase
{
    private readonly AppDbContext _db;
    public HolesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await EnsureHolesSeededAsync();
        var holes = await _db.Holes.OrderBy(h => h.HoleNumber).ToListAsync();
        return Ok(holes.Select(ToResponse));
    }

    [HttpGet("{number:int}")]
    public async Task<IActionResult> GetByNumber(int number)
    {
        await EnsureHolesSeededAsync();
        var hole = await _db.Holes.FirstOrDefaultAsync(h => h.HoleNumber == number);
        if (hole == null) return NotFound();
        return Ok(ToResponse(hole));
    }

    [HttpPut("{number:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int number, [FromBody] UpdateHoleRequest request)
    {
        await EnsureHolesSeededAsync();
        var hole = await _db.Holes.FirstOrDefaultAsync(h => h.HoleNumber == number);
        if (hole == null) return NotFound();

        hole.Par = request.Par;
        hole.Handicap = request.Handicap;
        hole.YardageBlue = request.YardageBlue;
        hole.YardageWhite = request.YardageWhite;
        hole.YardageRed = request.YardageRed;
        hole.Description = request.Description;
        hole.ImageUrl = request.ImageUrl;
        hole.DiagramUrl = request.DiagramUrl;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(hole));
    }

    private async Task EnsureHolesSeededAsync()
    {
        if (await _db.Holes.AnyAsync()) return;
        var holes = Enumerable.Range(1, 18).Select(n => new Hole
        {
            Id = Guid.NewGuid(),
            HoleNumber = n,
            Par = n % 3 == 0 ? 5 : n % 3 == 1 ? 4 : 3,
            Handicap = n,
            Description = string.Empty
        });
        _db.Holes.AddRange(holes);
        await _db.SaveChangesAsync();
    }

    private static HoleResponse ToResponse(Hole h) =>
        new(h.Id, h.HoleNumber, h.Par, h.Handicap,
            h.YardageBlue, h.YardageWhite, h.YardageRed,
            h.Description, h.ImageUrl, h.DiagramUrl);
}
