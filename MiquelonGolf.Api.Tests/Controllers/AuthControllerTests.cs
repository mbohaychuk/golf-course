// MiquelonGolf.Api.Tests/Controllers/AuthControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class AuthControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebAppFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidRequest_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "newuser@test.com",
            password = "Password1!",
            firstName = "John",
            lastName = "Smith",
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body?.Token);
        Assert.Equal("Admin", body?.Role);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "login@test.com",
            password = "Password1!",
            firstName = "Jane",
            lastName = "Doe",
            role = "Admin"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login@test.com",
            password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body?.Token);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "badlogin@test.com",
            password = "Password1!",
            firstName = "Bad",
            lastName = "Login",
            role = "Admin"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "badlogin@test.com",
            password = "WrongPassword1!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record AuthResponseDto(string Token, string UserId, string Role);
}
