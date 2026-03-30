// MiquelonGolf.Api.Tests/Helpers/AuthHelpers.cs
using System.Net.Http.Json;

namespace MiquelonGolf.Api.Tests.Helpers;

public static class AuthHelpers
{
    public static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@test.com",
            password = "Admin1234!"
        });

        if (!login.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Admin login failed: {await login.Content.ReadAsStringAsync()}");

        var response = await login.Content.ReadFromJsonAsync<AuthResponse>();
        return response!.Token;
    }

    private record AuthResponse(string Token, string UserId, string Role);
}
