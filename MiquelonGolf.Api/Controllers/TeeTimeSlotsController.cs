// MiquelonGolf.Api/Controllers/TeeTimeSlotsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.TeeTimeSlots;
using MiquelonGolf.Api.Services;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/tee-time-slots")]
public class TeeTimeSlotsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITeeTimeService _teeTimeService;

    public TeeTimeSlotsController(AppDbContext db, ITeeTimeService teeTimeService)
    {
        _db = db;
        _teeTimeService = teeTimeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByDate([FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        var slots = await _db.TeeTimeSlots
            .Include(s => s.Bookings)
            .Where(s => s.Date == parsedDate)
            .OrderBy(s => s.StartTime)
            .Select(s => new TeeTimeSlotResponse(
                s.Id,
                s.Date.ToString("yyyy-MM-dd"),
                s.StartTime.ToString("HH:mm"),
                s.MaxPlayers,
                s.IsBlocked,
                s.BlockReason,
                s.Bookings.Count(b => b.Status == Models.BookingStatus.Confirmed),
                s.StartingHole))
            .ToListAsync();

        return Ok(slots);
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateSlots([FromBody] GenerateSlotsRequest request)
    {
        if (!DateOnly.TryParse(request.Date, out var date))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        if (!TimeOnly.TryParse(request.OpenTime, out var openTime))
            return BadRequest("Invalid openTime format. Use HH:mm.");
        if (!TimeOnly.TryParse(request.CloseTime, out var closeTime))
            return BadRequest("Invalid closeTime format. Use HH:mm.");
        if (request.IntervalMinutes <= 0)
            return BadRequest("intervalMinutes must be greater than 0.");
        if (openTime >= closeTime)
            return BadRequest("openTime must be earlier than closeTime.");

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var existing = await _db.TeeTimeSlots
            .Include(s => s.Bookings)
            .Where(s => s.Date == date && !s.Bookings.Any())
            .ToListAsync();
        _db.TeeTimeSlots.RemoveRange(existing);

        var slots = _teeTimeService.GenerateSlots(
            date, request.IntervalMinutes, openTime, closeTime, request.MaxPlayers);
        _db.TeeTimeSlots.AddRange(slots);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var response = slots.Select(s => new TeeTimeSlotResponse(
            s.Id, s.Date.ToString("yyyy-MM-dd"), s.StartTime.ToString("HH:mm"),
            s.MaxPlayers, s.IsBlocked, s.BlockReason, 0, s.StartingHole));
        return Ok(response);
    }

    [HttpPatch("{id:guid}/block")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BlockSlot(Guid id, [FromBody] BlockSlotRequest request)
    {
        var slot = await _db.TeeTimeSlots.FindAsync(id);
        if (slot == null) return NotFound();
        slot.IsBlocked = true;
        slot.BlockReason = request.Reason;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/unblock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnblockSlot(Guid id)
    {
        var slot = await _db.TeeTimeSlots.FindAsync(id);
        if (slot == null) return NotFound();
        slot.IsBlocked = false;
        slot.BlockReason = null;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
