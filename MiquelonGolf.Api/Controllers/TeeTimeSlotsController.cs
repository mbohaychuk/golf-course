using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.TeeTimeSlots;
using MiquelonGolf.Api.Models;
using MiquelonGolf.Api.Services;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/tee-time-slots")]
public class TeeTimeSlotsController(AppDbContext db, ITeeTimeService teeTimeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TeeTimeSlotResponse>>> GetByDate(
        [FromQuery] string date,
        [FromQuery] int startingHole = 1)
    {
        if (!DateOnly.TryParse(date, CultureInfo.InvariantCulture, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        if (startingHole != 1 && startingHole != 10)
            return BadRequest("Starting hole must be 1 or 10.");

        var slots = await db.TeeTimeSlots
            .Include(s => s.Bookings)
            .Where(s => s.Date == parsedDate && s.StartingHole == startingHole)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return Ok(slots.Select(s => new TeeTimeSlotResponse(
            s.Id,
            s.Date.ToString("yyyy-MM-dd"),
            s.StartTime.ToString("HH:mm"),
            s.MaxPlayers,
            s.IsBlocked,
            s.BlockReason,
            s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
            s.StartingHole
        )).ToList());
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TeeTimeSlotResponse>>> Generate(GenerateSlotsRequest request)
    {
        if (!DateOnly.TryParse(request.Date, CultureInfo.InvariantCulture, out var parsedDate))
            return BadRequest("Invalid date format.");

        if (request.IntervalMinutes <= 0)
            return BadRequest("Interval must be greater than 0.");

        var open = TimeOnly.Parse(request.OpenTime);
        var close = TimeOnly.Parse(request.CloseTime);
        if (open >= close)
            return BadRequest("Open time must be before close time.");

        await using var transaction = await db.Database.BeginTransactionAsync();

        // Remove existing unbooked slots for this date (both holes)
        var unbookedSlots = await db.TeeTimeSlots
            .Where(s => s.Date == parsedDate && !s.Bookings.Any())
            .ToListAsync();
        db.TeeTimeSlots.RemoveRange(unbookedSlots);

        // Generate for Hole 1
        var hole1Slots = teeTimeService.GenerateSlots(
            parsedDate, request.IntervalMinutes, open, close, request.MaxPlayers, 1);
        db.TeeTimeSlots.AddRange(hole1Slots);

        // Generate for Hole 10
        var hole10Slots = teeTimeService.GenerateSlots(
            parsedDate, request.IntervalMinutes, open, close, request.MaxPlayers, 10);
        db.TeeTimeSlots.AddRange(hole10Slots);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        var allSlots = hole1Slots.Concat(hole10Slots)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.StartingHole)
            .Select(s => new TeeTimeSlotResponse(
                s.Id,
                s.Date.ToString("yyyy-MM-dd"),
                s.StartTime.ToString("HH:mm"),
                s.MaxPlayers,
                s.IsBlocked,
                s.BlockReason,
                0,
                s.StartingHole
            ))
            .ToList();

        return Ok(allSlots);
    }

    [HttpPatch("{id:guid}/block")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Block(Guid id, BlockSlotRequest request)
    {
        var slot = await db.TeeTimeSlots.FindAsync(id);
        if (slot == null) return NotFound();
        slot.IsBlocked = true;
        slot.BlockReason = request.Reason;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/unblock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        var slot = await db.TeeTimeSlots.FindAsync(id);
        if (slot == null) return NotFound();
        slot.IsBlocked = false;
        slot.BlockReason = null;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("flow-through")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<FlowThroughEntry>>> GetFlowThrough([FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, CultureInfo.InvariantCulture, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        // Get turn time offset from settings
        var turnTimeSetting = await db.SiteContents.FindAsync("settings.turnTimeOffsetMinutes");
        var turnTimeMinutes = turnTimeSetting != null && int.TryParse(turnTimeSetting.Value, out var ttm) ? ttm : 135;

        // Find all 18-hole confirmed bookings on Hole 1 for this date
        var eighteenHoleBookings = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .Where(b => b.TeeTimeSlot.Date == parsedDate
                     && b.TeeTimeSlot.StartingHole == 1
                     && b.RoundType == RoundType.Eighteen
                     && b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.TeeTimeSlot.StartTime)
            .ToListAsync();

        var entries = eighteenHoleBookings.Select(b =>
        {
            var arrival = b.TeeTimeSlot.StartTime.AddMinutes(turnTimeMinutes);
            return new FlowThroughEntry(
                b.Id,
                b.GolferName,
                arrival.ToString("HH:mm"),
                b.TeeTimeSlot.StartTime.ToString("HH:mm"),
                b.NumberOfPlayers
            );
        }).ToList();

        return Ok(entries);
    }
}
