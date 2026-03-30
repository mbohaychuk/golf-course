namespace MiquelonGolf.Api.DTOs.Members;

public record CreateMemberRequest(
    string FirstName, string LastName,
    string? Email, string? Phone,
    string MembershipType, string MemberSince,
    int SeasonYear, string PurchaseDate, string ExpiryDate,
    bool CartTrackage, bool SeasonalCartRental);
