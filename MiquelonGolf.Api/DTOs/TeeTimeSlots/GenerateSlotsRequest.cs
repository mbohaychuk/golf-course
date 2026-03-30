// MiquelonGolf.Api/DTOs/TeeTimeSlots/GenerateSlotsRequest.cs
namespace MiquelonGolf.Api.DTOs.TeeTimeSlots;
public record GenerateSlotsRequest(
    string Date, int IntervalMinutes,
    string OpenTime, string CloseTime, int MaxPlayers);
