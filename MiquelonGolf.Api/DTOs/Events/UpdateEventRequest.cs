namespace MiquelonGolf.Api.DTOs.Events;
public record UpdateEventRequest(
    string Title, string Description, string EventDate,
    string? StartTime, bool IsPublic, string Category, string? ImageUrl = null);
