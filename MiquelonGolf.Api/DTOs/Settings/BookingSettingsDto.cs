using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Settings;

public record BookingSettingsDto(
    [Range(1, 60)] int BookingWindowDays,
    [Range(60, 240)] int TurnTimeOffsetMinutes,
    [Range(6, 20)] int TeeTimeIntervalMinutes,
    [Range(1, 8)] int MaxPlayersPerSlot
);
