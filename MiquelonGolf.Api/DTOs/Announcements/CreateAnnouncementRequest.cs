namespace MiquelonGolf.Api.DTOs.Announcements;
public record CreateAnnouncementRequest(
    string Message, bool IsActive, string Type, string? ExpiresAt = null);
