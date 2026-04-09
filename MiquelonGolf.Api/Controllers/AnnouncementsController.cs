// MiquelonGolf.Api/Controllers/AnnouncementsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Announcements;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/announcements")]
public class AnnouncementsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AnnouncementsController(AppDbContext db) => _db = db;

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var now = DateTime.UtcNow;
        var active = await _db.Announcements
            .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > now))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return Ok(active.Select(ToResponse));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var all = await _db.Announcements
            .OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Ok(all.Select(ToResponse));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request)
    {
        if (!Enum.TryParse<AnnouncementType>(request.Type, ignoreCase: true, out var type))
            return BadRequest("Invalid announcement type.");

        DateTime? expiresAt = null;
        if (request.ExpiresAt != null)
        {
            if (!DateTime.TryParse(request.ExpiresAt, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                return BadRequest("Invalid expiresAt datetime.");
            expiresAt = parsed.ToUniversalTime();
        }

        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Message = request.Message,
            IsActive = request.IsActive,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();
        return StatusCode(201, ToResponse(announcement));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementRequest request)
    {
        var announcement = await _db.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();
        if (!Enum.TryParse<AnnouncementType>(request.Type, ignoreCase: true, out var type))
            return BadRequest("Invalid announcement type.");

        DateTime? expiresAt = null;
        if (request.ExpiresAt != null)
        {
            if (!DateTime.TryParse(request.ExpiresAt, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                return BadRequest("Invalid expiresAt datetime.");
            expiresAt = parsed.ToUniversalTime();
        }

        announcement.Message = request.Message;
        announcement.IsActive = request.IsActive;
        announcement.Type = type;
        announcement.ExpiresAt = expiresAt;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(announcement));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var announcement = await _db.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();
        _db.Announcements.Remove(announcement);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static AnnouncementResponse ToResponse(Announcement a) =>
        new(a.Id, a.Message, a.IsActive, a.Type.ToString(), a.CreatedAt, a.ExpiresAt);
}
