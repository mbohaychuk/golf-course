namespace MiquelonGolf.Api.DTOs.TeeTimeSlots;

public record FlowThroughEntry(
    Guid BookingId,
    string GolferName,
    string EstimatedArrival,
    string OriginSlotTime,
    int NumberOfPlayers
);
