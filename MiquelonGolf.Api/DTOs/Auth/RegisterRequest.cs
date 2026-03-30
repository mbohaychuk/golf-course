// MiquelonGolf.Api/DTOs/Auth/RegisterRequest.cs
namespace MiquelonGolf.Api.DTOs.Auth;
public record RegisterRequest(
    string Email, string Password,
    string FirstName, string LastName,
    string Role, string? Phone = null);
