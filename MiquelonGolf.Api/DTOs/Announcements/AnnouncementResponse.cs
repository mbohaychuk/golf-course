namespace MiquelonGolf.Api.DTOs.Announcements;
public record AnnouncementResponse(
    Guid Id, string Message, bool IsActive,
    string Type, DateTime CreatedAt, DateTime? ExpiresAt);
