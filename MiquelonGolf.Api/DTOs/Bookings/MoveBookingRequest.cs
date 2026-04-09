using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Bookings;

public record MoveBookingRequest(
    [Required] Guid TargetTeeTimeSlotId
);
