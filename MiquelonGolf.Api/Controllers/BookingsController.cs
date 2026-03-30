// MiquelonGolf.Api/Controllers/BookingsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Bookings;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var slot = await _db.TeeTimeSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.TeeTimeSlotId);

        if (slot == null) return NotFound("Tee time slot not found.");
        if (slot.IsBlocked) return BadRequest("This slot is not available.");

        var confirmed = slot.Bookings.Count(b => b.Status == BookingStatus.Confirmed);
        if (confirmed + request.NumberOfPlayers > slot.MaxPlayers)
            return BadRequest("Not enough spots available in this slot.");

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
            Status = BookingStatus.Confirmed
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var response = ToResponse(booking, slot);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var booking = await _db.Bookings
            .Include(b => b.TeeTimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();
        return Ok(ToResponse(booking, booking.TeeTimeSlot));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByDate([FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        var bookings = await _db.Bookings
            .Include(b => b.TeeTimeSlot)
            .Where(b => b.TeeTimeSlot.Date == parsedDate)
            .OrderBy(b => b.TeeTimeSlot.StartTime)
            .ToListAsync();

        return Ok(bookings.Select(b => ToResponse(b, b.TeeTimeSlot)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static BookingResponse ToResponse(Booking b, TeeTimeSlot slot) =>
        new(b.Id, b.TeeTimeSlotId,
            slot.Date.ToString("yyyy-MM-dd"),
            slot.StartTime.ToString("HH:mm"),
            b.GolferName, b.GolferEmail, b.GolferPhone,
            b.NumberOfPlayers, b.NumberOfCarts,
            b.Status.ToString(), b.BookedAt);
}
