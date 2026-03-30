namespace MiquelonGolf.Api.DTOs.Members;

public record MemberResponse(
    Guid Id, string FirstName, string LastName,
    string? Email, string? Phone, string MembershipType,
    string MemberSince, int SeasonYear,
    string PurchaseDate, string ExpiryDate,
    bool CartTrackage, bool SeasonalCartRental);
