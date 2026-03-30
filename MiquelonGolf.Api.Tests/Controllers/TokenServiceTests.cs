// MiquelonGolf.Api.Tests/Controllers/TokenServiceTests.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiquelonGolf.Api.Models;
using MiquelonGolf.Api.Services;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class TokenServiceTests
{
    private readonly ITokenService _sut;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-test-key-that-is-long-enough",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();
        _sut = new TokenService(config);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt_WithCorrectClaims()
    {
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "test@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var token = _sut.GenerateToken(user, "Admin");

        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("super-secret-test-key-that-is-long-enough")),
            ValidateIssuer = true,
            ValidIssuer = "TestIssuer",
            ValidateAudience = true,
            ValidAudience = "TestAudience",
            ValidateLifetime = true,
        };
        var principal = handler.ValidateToken(token, validationParams, out var validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        Assert.Equal("user-123", jwtToken.Subject);
        Assert.Equal("Admin", principal.FindFirst(ClaimTypes.Role)?.Value);
        Assert.Equal("test@example.com", principal.FindFirst(ClaimTypes.Email)?.Value);
    }
}
