using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Services;

public interface ITeeTimeService
{
    IReadOnlyList<TeeTimeSlot> GenerateSlots(
        DateOnly date, int intervalMinutes,
        TimeOnly openTime, TimeOnly closeTime,
        int maxPlayers, int startingHole = 1);
}
