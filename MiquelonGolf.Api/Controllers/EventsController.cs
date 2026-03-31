// MiquelonGolf.Api/Controllers/EventsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Events;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;
    public EventsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var isAdmin = User.IsInRole("Admin");
        var query = _db.Events.AsQueryable();
        if (!isAdmin) query = query.Where(e => e.IsPublic);

        var events = await query.OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if (!ev.IsPublic && !User.IsInRole("Admin")) return Forbid();
        return Ok(ToResponse(ev));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        if (!Enum.TryParse<EventCategory>(request.Category, ignoreCase: true, out var category))
            return BadRequest("Invalid category.");
        if (!DateOnly.TryParse(request.EventDate, out var eventDate))
            return BadRequest("Invalid eventDate.");

        TimeOnly? startTime = null;
        if (request.StartTime != null)
        {
            if (!TimeOnly.TryParse(request.StartTime, out var parsed))
                return BadRequest("Invalid startTime.");
            startTime = parsed;
        }

        var ev = new GolfEvent
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            EventDate = eventDate,
            StartTime = startTime,
            IsPublic = request.IsPublic,
            Category = category,
            ImageUrl = request.ImageUrl
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ToResponse(ev));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if (!Enum.TryParse<EventCategory>(request.Category, ignoreCase: true, out var category))
            return BadRequest("Invalid category.");
        if (!DateOnly.TryParse(request.EventDate, out var eventDate))
            return BadRequest("Invalid eventDate.");

        TimeOnly? startTime = null;
        if (request.StartTime != null)
        {
            if (!TimeOnly.TryParse(request.StartTime, out var parsed))
                return BadRequest("Invalid startTime.");
            startTime = parsed;
        }

        ev.Title = request.Title;
        ev.Description = request.Description;
        ev.EventDate = eventDate;
        ev.StartTime = startTime;
        ev.IsPublic = request.IsPublic;
        ev.Category = category;
        ev.ImageUrl = request.ImageUrl;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(ev));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static EventResponse ToResponse(GolfEvent e) =>
        new(e.Id, e.Title, e.Description,
            e.EventDate.ToString("yyyy-MM-dd"),
            e.StartTime?.ToString("HH:mm"),
            e.IsPublic, e.Category.ToString(), e.ImageUrl);
}
