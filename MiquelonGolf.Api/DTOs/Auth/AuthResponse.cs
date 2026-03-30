// MiquelonGolf.Api/DTOs/Auth/AuthResponse.cs
namespace MiquelonGolf.Api.DTOs.Auth;
public record AuthResponse(string Token, string UserId, string Role);
