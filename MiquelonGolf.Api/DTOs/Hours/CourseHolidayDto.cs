namespace MiquelonGolf.Api.DTOs.Hours;

public record CourseHolidayDto(Guid Id, string Date, string Reason);

public record CreateHolidayRequest(string Date, string Reason);
