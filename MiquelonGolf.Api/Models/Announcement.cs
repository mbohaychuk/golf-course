namespace MiquelonGolf.Api.Models;

public class Announcement
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public AnnouncementType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
