// MiquelonGolf.Api/Services/ITeeTimeService.cs
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Services;

public interface ITeeTimeService
{
    IEnumerable<TeeTimeSlot> GenerateSlots(
        DateOnly date, int intervalMinutes,
        TimeOnly openTime, TimeOnly closeTime, int maxPlayers);
}
