namespace MiquelonGolf.Api.Models;

public class SpecialHours
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}
