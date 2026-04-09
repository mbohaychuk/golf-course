using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Members;

public record UpdateMemberRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string? Email,
    string? Phone,
    string MembershipType,
    int SeasonYear, string PurchaseDate, string ExpiryDate,
    bool CartTrackage, bool SeasonalCartRental);
