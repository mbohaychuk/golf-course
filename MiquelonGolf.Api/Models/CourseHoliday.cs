namespace MiquelonGolf.Api.Models;

public class CourseHoliday
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Reason { get; set; } = string.Empty;
}
