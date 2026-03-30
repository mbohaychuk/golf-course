namespace MiquelonGolf.Api.Models;

public class Member
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public MembershipType MembershipType { get; set; }
    public DateOnly MemberSince { get; set; }
    public int SeasonYear { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public bool CartTrackage { get; set; }
    public bool SeasonalCartRental { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
