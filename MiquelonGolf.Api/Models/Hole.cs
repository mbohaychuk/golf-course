namespace MiquelonGolf.Api.Models;

public class Hole
{
    public Guid Id { get; set; }
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int Handicap { get; set; }
    public int YardageBlue { get; set; }
    public int YardageWhite { get; set; }
    public int YardageRed { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? DiagramUrl { get; set; }
}
