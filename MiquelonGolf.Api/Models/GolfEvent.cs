namespace MiquelonGolf.Api.Models;

public class GolfEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public bool IsPublic { get; set; } = true;
    public EventCategory Category { get; set; }
    public string? ImageUrl { get; set; }
}
