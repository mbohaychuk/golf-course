namespace MiquelonGolf.Api.DTOs.Announcements;
public record UpdateAnnouncementRequest(
    string Message, bool IsActive, string Type, string? ExpiresAt = null);
