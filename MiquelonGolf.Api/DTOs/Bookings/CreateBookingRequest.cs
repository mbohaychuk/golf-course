// MiquelonGolf.Api/DTOs/Bookings/CreateBookingRequest.cs
namespace MiquelonGolf.Api.DTOs.Bookings;
public record CreateBookingRequest(
    Guid TeeTimeSlotId, string GolferName,
    string GolferEmail, string GolferPhone,
    int NumberOfPlayers, int NumberOfCarts);
