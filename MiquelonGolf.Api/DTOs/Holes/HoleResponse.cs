namespace MiquelonGolf.Api.DTOs.Holes;
public record HoleResponse(
    Guid Id, int HoleNumber, int Par, int Handicap,
    int YardageBlue, int YardageWhite, int YardageRed,
    string Description, string? ImageUrl, string? DiagramUrl);
