// MiquelonGolf.Api.Tests/Helpers/AuthHelpers.cs
using System.Net.Http.Json;

namespace MiquelonGolf.Api.Tests.Helpers;

public static class AuthHelpers
{
    public static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        // Register then log in as admin — seeded in factory or registered on first call
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "admin@test.com",
            password = "Admin1234!",
            firstName = "Admin",
            lastName = "User",
            role = "Admin"
        });

        // If already registered (409), that's fine — just log in
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@test.com",
            password = "Admin1234!"
        });

        login.EnsureSuccessStatusCode();
        var response = await login.Content.ReadFromJsonAsync<AuthResponse>();
        return response!.Token;
    }

    private record AuthResponse(string Token, string UserId, string Role);
}
