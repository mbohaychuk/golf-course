namespace MiquelonGolf.Api.DTOs.Events;
public record EventResponse(
    Guid Id, string Title, string Description,
    string EventDate, string? StartTime,
    bool IsPublic, string Category, string? ImageUrl);
