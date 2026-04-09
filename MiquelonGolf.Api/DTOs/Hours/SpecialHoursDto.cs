namespace MiquelonGolf.Api.DTOs.Hours;

public record SpecialHoursDto(Guid Id, string Date, string OpenTime, string CloseTime, string Reason);

public record UpsertSpecialHoursRequest(string Date, string OpenTime, string CloseTime, string Reason);
