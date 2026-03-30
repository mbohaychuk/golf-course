// MiquelonGolf.Api/DTOs/Bookings/BookingResponse.cs
namespace MiquelonGolf.Api.DTOs.Bookings;
public record BookingResponse(
    Guid Id, Guid TeeTimeSlotId, string SlotDate, string SlotTime,
    string GolferName, string GolferEmail, string GolferPhone,
    int NumberOfPlayers, int NumberOfCarts,
    string Status, DateTime BookedAt);
