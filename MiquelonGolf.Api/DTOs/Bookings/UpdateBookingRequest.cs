using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Bookings;

public record UpdateBookingRequest(
    [Required, MinLength(1), MaxLength(100)] string GolferName,
    [Required, EmailAddress] string GolferEmail,
    [Required, MinLength(10)] string GolferPhone,
    [Range(1, 4)] int NumberOfPlayers,
    [Range(0, 4)] int NumberOfCarts
);
