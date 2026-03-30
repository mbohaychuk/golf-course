// MiquelonGolf.Api.Tests/Controllers/TeeTimeSlotsControllerTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class TeeTimeSlotsControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public TeeTimeSlotsControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateSlots_AsAdmin_CreatesSlots()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/tee-time-slots/generate", new
        {
            date = "2026-06-01",
            intervalMinutes = 10,
            openTime = "08:00",
            closeTime = "10:00",
            maxPlayers = 4
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var slots = await response.Content
            .ReadFromJsonAsync<List<TeeTimeSlotDto>>();
        Assert.Equal(12, slots!.Count); // 08:00–10:00 @ 10min = 12 slots

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetSlots_ByDate_ReturnsSlots()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/tee-time-slots/generate", new
        {
            date = "2026-06-02",
            intervalMinutes = 10,
            openTime = "08:00",
            closeTime = "09:00",
            maxPlayers = 4
        });

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/tee-time-slots?date=2026-06-02");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var slots = await response.Content
            .ReadFromJsonAsync<List<TeeTimeSlotDto>>();
        Assert.True(slots!.Count > 0);
    }

    [Fact]
    public async Task GenerateSlots_AsPublic_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/tee-time-slots/generate", new
        {
            date = "2026-06-03",
            intervalMinutes = 10,
            openTime = "08:00",
            closeTime = "10:00",
            maxPlayers = 4
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record TeeTimeSlotDto(
        Guid Id, string Date, string StartTime,
        int MaxPlayers, bool IsBlocked,
        string? BlockReason, int BookingCount);
}
