namespace MiquelonGolf.Api.Models;

public class SiteContent
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
}
