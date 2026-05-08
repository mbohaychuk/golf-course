using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Bookings;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingConfirmation>> Create(CreateBookingRequest request)
    {
        if (request.NumberOfCarts > request.NumberOfPlayers)
            return BadRequest("Number of carts cannot exceed number of players.");

        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        var slot = await db.TeeTimeSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.TeeTimeSlotId);

        if (slot == null)
            return NotFound("Tee time slot not found.");

        if (slot.IsBlocked)
            return BadRequest("This slot is not available.");

        // Validate round type matches starting hole
        var expectedHole = request.RoundType == RoundType.BackNine ? 10 : 1;
        if (slot.StartingHole != expectedHole)
            return BadRequest($"Round type {request.RoundType} requires starting hole {expectedHole}, but slot starts on hole {slot.StartingHole}.");

        // Validate slot is not in the past
        var slotDateTime = slot.Date.ToDateTime(slot.StartTime);
        if (slotDateTime < DateTime.Now)
            return BadRequest("Cannot book a slot in the past.");

        // Validate booking window
        var windowContent = await db.SiteContents.FindAsync("settings.bookingWindowDays");
        var windowDays = windowContent != null && int.TryParse(windowContent.Value, out var wd) ? wd : 14;
        if (slot.Date > DateOnly.FromDateTime(DateTime.Today.AddDays(windowDays)))
            return BadRequest("This date is outside the booking window.");

        // Validate not a holiday
        var isHoliday = await db.CourseHolidays.AnyAsync(h => h.Date == slot.Date);
        if (isHoliday)
            return BadRequest("The course is closed on this date.");

        var confirmed = slot.Bookings.Count(b => b.Status == BookingStatus.Confirmed);
        if (confirmed + request.NumberOfPlayers > slot.MaxPlayers)
            return Conflict("Not enough spots available in this slot.");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            TeeTimeSlotId = request.TeeTimeSlotId,
            BookedAt = DateTime.UtcNow,
            GolferName = request.GolferName,
            GolferEmail = request.GolferEmail,
            GolferPhone = request.GolferPhone,
            NumberOfPlayers = request.NumberOfPlayers,
            NumberOfCarts = request.NumberOfCarts,
            Status = BookingStatus.Confirmed,
            RoundType = request.RoundType,
            ReferralSource = request.ReferralSource,
            ConfirmationCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return StatusCode(201, new BookingConfirmation(
            booking.Id,
            booking.ConfirmationCode,
            slot.Date.ToString("yyyy-MM-dd"),
            slot.StartTime.ToString("HH:mm"),
            slot.StartingHole,
            booking.GolferName,
            booking.GolferEmail,
            booking.GolferPhone,
            booking.NumberOfPlayers,
            booking.NumberOfCarts,
            booking.Status.ToString(),
            booking.RoundType.ToString()
        ));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id)
    {
        var booking = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        return Ok(ToResponse(booking));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<BookingResponse>>> GetByDate([FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, CultureInfo.InvariantCulture, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        var bookings = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .Where(b => b.TeeTimeSlot.Date == parsedDate)
            .OrderBy(b => b.TeeTimeSlot.StartTime)
            .ThenBy(b => b.TeeTimeSlot.StartingHole)
            .ToListAsync();

        return Ok(bookings.Select(ToResponse).ToList());
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookingResponse>> Edit(Guid id, UpdateBookingRequest request)
    {
        if (request.NumberOfCarts > request.NumberOfPlayers)
            return BadRequest("Number of carts cannot exceed number of players.");

        var booking = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .ThenInclude(s => s.Bookings)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        if (booking.Status != BookingStatus.Confirmed)
            return BadRequest("Can only edit confirmed bookings.");

        // Check capacity if player count increased
        if (request.NumberOfPlayers > booking.NumberOfPlayers)
        {
            var confirmed = booking.TeeTimeSlot.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed && b.Id != id)
                .Sum(b => b.NumberOfPlayers);

            if (confirmed + request.NumberOfPlayers > booking.TeeTimeSlot.MaxPlayers)
                return Conflict("Not enough spots available for the increased player count.");
        }

        booking.GolferName = request.GolferName;
        booking.GolferEmail = request.GolferEmail;
        booking.GolferPhone = request.GolferPhone;
        booking.NumberOfPlayers = request.NumberOfPlayers;
        booking.NumberOfCarts = request.NumberOfCarts;
        await db.SaveChangesAsync();

        return Ok(ToResponse(booking));
    }

    [HttpPost("{id:guid}/move")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookingResponse>> Move(Guid id, MoveBookingRequest request)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        var booking = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        if (booking.Status != BookingStatus.Confirmed)
            return BadRequest("Can only move confirmed bookings.");

        var targetSlot = await db.TeeTimeSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.TargetTeeTimeSlotId);

        if (targetSlot == null) return NotFound("Target slot not found.");
        if (targetSlot.IsBlocked) return BadRequest("Target slot is blocked.");

        // Validate round type matches target slot
        var expectedHole = booking.RoundType == RoundType.BackNine ? 10 : 1;
        if (targetSlot.StartingHole != expectedHole)
            return BadRequest($"Cannot move to a slot with starting hole {targetSlot.StartingHole}. Booking requires hole {expectedHole}.");

        var confirmed = targetSlot.Bookings.Count(b => b.Status == BookingStatus.Confirmed);
        if (confirmed + booking.NumberOfPlayers > targetSlot.MaxPlayers)
            return Conflict("Not enough spots in the target slot.");

        booking.TeeTimeSlotId = request.TargetTeeTimeSlotId;
        booking.TeeTimeSlot = targetSlot;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(ToResponse(booking));
    }

    [HttpPatch("{id:guid}/no-show")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkNoShow(Guid id)
    {
        var booking = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        if (booking.Status != BookingStatus.Confirmed)
            return BadRequest("Can only mark confirmed bookings as no-show.");

        var slotDateTime = booking.TeeTimeSlot.Date.ToDateTime(booking.TeeTimeSlot.StartTime);
        if (slotDateTime > DateTime.Now)
            return BadRequest("Cannot mark a future booking as no-show.");

        booking.Status = BookingStatus.NoShow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<BookingSearchResult>>> Search(
        [FromQuery] string q,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query is required.");

        var query = q.Trim().ToLower();
        var results = await db.Bookings
            .Include(b => b.TeeTimeSlot)
            .Where(b => b.GolferName.ToLower().Contains(query)
                     || b.GolferEmail.ToLower().Contains(query)
                     || b.ConfirmationCode.ToLower().Contains(query))
            .OrderByDescending(b => b.TeeTimeSlot.Date)
            .ThenBy(b => b.TeeTimeSlot.StartTime)
            .Skip(skip)
            .Take(take)
            .Select(b => new BookingSearchResult(
                b.Id,
                b.TeeTimeSlot.Date.ToString("yyyy-MM-dd"),
                b.TeeTimeSlot.StartTime.ToString("HH:mm"),
                b.TeeTimeSlot.StartingHole,
                b.GolferName,
                b.GolferEmail,
                b.Status.ToString(),
                b.RoundType.ToString(),
                b.ConfirmationCode
            ))
            .ToListAsync();

        return Ok(results);
    }

    private static BookingResponse ToResponse(Booking b) => new(
        b.Id,
        b.TeeTimeSlotId,
        b.TeeTimeSlot.Date.ToString("yyyy-MM-dd"),
        b.TeeTimeSlot.StartTime.ToString("HH:mm"),
        b.TeeTimeSlot.StartingHole,
        b.GolferName,
        b.GolferEmail,
        b.GolferPhone,
        b.NumberOfPlayers,
        b.NumberOfCarts,
        b.Status.ToString(),
        b.RoundType.ToString(),
        b.ReferralSource,
        b.ConfirmationCode,
        b.BookedAt
    );
}
