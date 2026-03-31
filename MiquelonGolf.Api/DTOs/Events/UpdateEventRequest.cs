using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Events;
public record UpdateEventRequest(
    [Required] string Title,
    [Required] string Description,
    string EventDate,
    string? StartTime,
    bool IsPublic,
    string Category,
    string? ImageUrl = null);
