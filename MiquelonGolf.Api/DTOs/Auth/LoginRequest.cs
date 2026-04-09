// MiquelonGolf.Api/DTOs/Auth/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.DTOs.Auth;
public record LoginRequest(
    [Required] string Email,
    [Required] string Password);
