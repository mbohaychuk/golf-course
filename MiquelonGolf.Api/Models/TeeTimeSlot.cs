namespace MiquelonGolf.Api.Models;

public class TeeTimeSlot
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public int MaxPlayers { get; set; } = 4;
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
