using Microsoft.AspNetCore.Identity;

namespace MiquelonGolf.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
}
