// MiquelonGolf.Api/Services/TeeTimeService.cs
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Services;

public class TeeTimeService : ITeeTimeService
{
    public IReadOnlyList<TeeTimeSlot> GenerateSlots(
        DateOnly date, int intervalMinutes,
        TimeOnly openTime, TimeOnly closeTime, int maxPlayers)
    {
        var slots = new List<TeeTimeSlot>();
        var current = openTime;
        while (current < closeTime)
        {
            slots.Add(new TeeTimeSlot
            {
                Id = Guid.NewGuid(),
                Date = date,
                StartTime = current,
                MaxPlayers = maxPlayers
            });
            current = current.AddMinutes(intervalMinutes);
        }
        return slots;
    }
}
