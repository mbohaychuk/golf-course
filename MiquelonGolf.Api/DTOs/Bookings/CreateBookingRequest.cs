// MiquelonGolf.Api/DTOs/Bookings/CreateBookingRequest.cs
using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Bookings;

public record CreateBookingRequest(
    Guid TeeTimeSlotId,
    [Required, MinLength(1)] string GolferName,
    [Required, EmailAddress] string GolferEmail,
    [Required, MinLength(1)] string GolferPhone,
    [Range(1, 4)] int NumberOfPlayers,
    [Range(0, 4)] int NumberOfCarts);
