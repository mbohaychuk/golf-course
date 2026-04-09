namespace MiquelonGolf.Api.DTOs.Bookings;

public record BookingSearchResult(
    Guid Id,
    string SlotDate,
    string SlotTime,
    int StartingHole,
    string GolferName,
    string GolferEmail,
    string Status,
    string RoundType,
    string ConfirmationCode
);
