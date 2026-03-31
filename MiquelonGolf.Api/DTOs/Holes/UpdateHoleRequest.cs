namespace MiquelonGolf.Api.DTOs.Holes;
public record UpdateHoleRequest(
    int Par, int Handicap,
    int YardageBlue, int YardageWhite, int YardageRed,
    string Description, string? ImageUrl, string? DiagramUrl);
