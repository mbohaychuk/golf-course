using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Members;

public record CreateMemberRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string? Email,
    string? Phone,
    string MembershipType, string MemberSince,
    int SeasonYear, string PurchaseDate, string ExpiryDate,
    bool CartTrackage, bool SeasonalCartRental);
