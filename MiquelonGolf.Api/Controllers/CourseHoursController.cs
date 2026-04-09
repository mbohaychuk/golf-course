// MiquelonGolf.Api/Controllers/CourseHoursController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Hours;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/course-hours")]
public class CourseHoursController : ControllerBase
{
    private static readonly string[] DayNames =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private readonly AppDbContext _db;

    public CourseHoursController(AppDbContext db) => _db = db;

    // ── Operating Hours (weekly schedule) ─────────────────────────────────────

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule()
    {
        var hours = await _db.OperatingHours.OrderBy(h => h.DayOfWeek).ToListAsync();
        var result = hours.Select(h => new OperatingHoursDto(
            h.DayOfWeek, DayNames[h.DayOfWeek], h.IsOpen,
            h.OpenTime.ToString("HH:mm"), h.CloseTime.ToString("HH:mm"),
            h.IntervalMinutes, h.MaxPlayers));
        return Ok(result);
    }

    [HttpPut("schedule/{dayOfWeek:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDay(int dayOfWeek, [FromBody] UpdateOperatingHoursRequest req)
    {
        if (dayOfWeek < 0 || dayOfWeek > 6) return BadRequest("dayOfWeek must be 0–6.");
        if (!TimeOnly.TryParse(req.OpenTime, out var open)) return BadRequest("Invalid openTime.");
        if (!TimeOnly.TryParse(req.CloseTime, out var close)) return BadRequest("Invalid closeTime.");
        if (req.IsOpen && open >= close) return BadRequest("openTime must be before closeTime.");
        if (req.IntervalMinutes < 5) return BadRequest("intervalMinutes must be at least 5.");
        if (req.MaxPlayers < 1) return BadRequest("maxPlayers must be at least 1.");

        var row = await _db.OperatingHours.FindAsync(dayOfWeek);
        if (row == null) return NotFound();

        row.IsOpen = req.IsOpen;
        row.OpenTime = open;
        row.CloseTime = close;
        row.IntervalMinutes = req.IntervalMinutes;
        row.MaxPlayers = req.MaxPlayers;
        await _db.SaveChangesAsync();

        return Ok(new OperatingHoursDto(
            row.DayOfWeek, DayNames[row.DayOfWeek], row.IsOpen,
            row.OpenTime.ToString("HH:mm"), row.CloseTime.ToString("HH:mm"),
            row.IntervalMinutes, row.MaxPlayers));
    }

    // ── Holidays (closed dates) ────────────────────────────────────────────────

    [HttpGet("holidays")]
    public async Task<IActionResult> GetHolidays()
    {
        var list = await _db.CourseHolidays
            .OrderBy(h => h.Date)
            .Select(h => new CourseHolidayDto(h.Id, h.Date.ToString("yyyy-MM-dd"), h.Reason))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("holidays")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest req)
    {
        if (!DateOnly.TryParse(req.Date, out var date)) return BadRequest("Invalid date.");
        if (string.IsNullOrWhiteSpace(req.Reason)) return BadRequest("Reason is required.");

        if (await _db.CourseHolidays.AnyAsync(h => h.Date == date))
            return Conflict("A holiday already exists for that date.");

        var holiday = new CourseHoliday { Id = Guid.NewGuid(), Date = date, Reason = req.Reason.Trim() };
        _db.CourseHolidays.Add(holiday);
        await _db.SaveChangesAsync();
        return Created($"/api/course-hours/holidays/{holiday.Id}",
            new CourseHolidayDto(holiday.Id, holiday.Date.ToString("yyyy-MM-dd"), holiday.Reason));
    }

    [HttpDelete("holidays/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHoliday(Guid id)
    {
        var holiday = await _db.CourseHolidays.FindAsync(id);
        if (holiday == null) return NotFound();
        _db.CourseHolidays.Remove(holiday);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── Special Hours (date overrides) ────────────────────────────────────────

    [HttpGet("special")]
    public async Task<IActionResult> GetSpecialHours()
    {
        var list = await _db.SpecialHours
            .OrderBy(s => s.Date)
            .Select(s => new SpecialHoursDto(
                s.Id, s.Date.ToString("yyyy-MM-dd"),
                s.OpenTime.ToString("HH:mm"), s.CloseTime.ToString("HH:mm"), s.Reason))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("special")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSpecialHours([FromBody] UpsertSpecialHoursRequest req)
    {
        if (!DateOnly.TryParse(req.Date, out var date)) return BadRequest("Invalid date.");
        if (!TimeOnly.TryParse(req.OpenTime, out var open)) return BadRequest("Invalid openTime.");
        if (!TimeOnly.TryParse(req.CloseTime, out var close)) return BadRequest("Invalid closeTime.");
        if (open >= close) return BadRequest("openTime must be before closeTime.");

        if (await _db.SpecialHours.AnyAsync(s => s.Date == date))
            return Conflict("Special hours already exist for that date. Use PUT to update.");

        var row = new SpecialHours
        {
            Id = Guid.NewGuid(), Date = date,
            OpenTime = open, CloseTime = close, Reason = req.Reason.Trim()
        };
        _db.SpecialHours.Add(row);
        await _db.SaveChangesAsync();
        return Created($"/api/course-hours/special/{row.Id}",
            new SpecialHoursDto(row.Id, row.Date.ToString("yyyy-MM-dd"),
                row.OpenTime.ToString("HH:mm"), row.CloseTime.ToString("HH:mm"), row.Reason));
    }

    [HttpPut("special/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSpecialHours(Guid id, [FromBody] UpsertSpecialHoursRequest req)
    {
        if (!TimeOnly.TryParse(req.OpenTime, out var open)) return BadRequest("Invalid openTime.");
        if (!TimeOnly.TryParse(req.CloseTime, out var close)) return BadRequest("Invalid closeTime.");
        if (open >= close) return BadRequest("openTime must be before closeTime.");

        var row = await _db.SpecialHours.FindAsync(id);
        if (row == null) return NotFound();
        row.OpenTime = open;
        row.CloseTime = close;
        row.Reason = req.Reason.Trim();
        await _db.SaveChangesAsync();
        return Ok(new SpecialHoursDto(row.Id, row.Date.ToString("yyyy-MM-dd"),
            row.OpenTime.ToString("HH:mm"), row.CloseTime.ToString("HH:mm"), row.Reason));
    }

    [HttpDelete("special/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSpecialHours(Guid id)
    {
        var row = await _db.SpecialHours.FindAsync(id);
        if (row == null) return NotFound();
        _db.SpecialHours.Remove(row);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
