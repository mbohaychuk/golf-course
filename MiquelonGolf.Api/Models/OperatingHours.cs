using System.ComponentModel.DataAnnotations.Schema;

namespace MiquelonGolf.Api.Models;

public class OperatingHours
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int DayOfWeek { get; set; }   // 0=Sunday … 6=Saturday (primary key)
    public bool IsOpen { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public int IntervalMinutes { get; set; } = 10;
    public int MaxPlayers { get; set; } = 4;
}
