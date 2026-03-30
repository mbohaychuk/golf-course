// MiquelonGolf.Api/DTOs/TeeTimeSlots/TeeTimeSlotResponse.cs
namespace MiquelonGolf.Api.DTOs.TeeTimeSlots;
public record TeeTimeSlotResponse(
    Guid Id, string Date, string StartTime,
    int MaxPlayers, bool IsBlocked,
    string? BlockReason, int BookingCount);
