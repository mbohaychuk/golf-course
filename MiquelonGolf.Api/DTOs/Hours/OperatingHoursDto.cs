namespace MiquelonGolf.Api.DTOs.Hours;

public record OperatingHoursDto(
    int DayOfWeek,
    string DayName,
    bool IsOpen,
    string OpenTime,
    string CloseTime,
    int IntervalMinutes,
    int MaxPlayers);

public record UpdateOperatingHoursRequest(
    bool IsOpen,
    string OpenTime,
    string CloseTime,
    int IntervalMinutes,
    int MaxPlayers);
