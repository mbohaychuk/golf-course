// MiquelonGolf.Api/Services/ITokenService.cs
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user, string role);
}
