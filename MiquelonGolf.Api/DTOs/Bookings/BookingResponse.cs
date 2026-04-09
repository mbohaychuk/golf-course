namespace MiquelonGolf.Api.DTOs.Bookings;

public record BookingResponse(
    Guid Id,
    Guid TeeTimeSlotId,
    string SlotDate,
    string SlotTime,
    int StartingHole,
    string GolferName,
    string GolferEmail,
    string GolferPhone,
    int NumberOfPlayers,
    int NumberOfCarts,
    string Status,
    string RoundType,
    string? ReferralSource,
    string ConfirmationCode,
    DateTime BookedAt
);

public record BookingConfirmation(
    Guid Id,
    string ConfirmationCode,
    string SlotDate,
    string SlotTime,
    int StartingHole,
    int NumberOfPlayers,
    int NumberOfCarts,
    string Status,
    string RoundType
);
