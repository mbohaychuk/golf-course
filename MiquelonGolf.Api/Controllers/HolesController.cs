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
        var holes = await _db.Holes.OrderBy(h => h.HoleNumber).ToListAsync();
        return Ok(holes.Select(ToResponse));
    }

    [HttpGet("{number:int}")]
    public async Task<IActionResult> GetByNumber(int number)
    {
        var hole = await _db.Holes.FirstOrDefaultAsync(h => h.HoleNumber == number);
        if (hole == null) return NotFound();
        return Ok(ToResponse(hole));
    }

    [HttpPut("{number:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int number, [FromBody] UpdateHoleRequest request)
    {
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

    private static HoleResponse ToResponse(Hole h) =>
        new(h.Id, h.HoleNumber, h.Par, h.Handicap,
            h.YardageBlue, h.YardageWhite, h.YardageRed,
            h.Description, h.ImageUrl, h.DiagramUrl);
}
