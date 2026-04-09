// MiquelonGolf.Api/DTOs/Auth/RegisterRequest.cs
using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Auth;
public record RegisterRequest(
    [Required, MaxLength(256)] string Email,
    [Required] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    string? Phone = null);
