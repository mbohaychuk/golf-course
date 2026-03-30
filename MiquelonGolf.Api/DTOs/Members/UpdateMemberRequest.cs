namespace MiquelonGolf.Api.DTOs.Members;

public record UpdateMemberRequest(
    string FirstName, string LastName,
    string? Email, string? Phone,
    string MembershipType,
    int SeasonYear, string PurchaseDate, string ExpiryDate,
    bool CartTrackage, bool SeasonalCartRental);
