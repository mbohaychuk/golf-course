// MiquelonGolf.Api.Tests/Controllers/BookingsControllerTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class BookingsControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public BookingsControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    private async Task<Guid> SeedSlotAsync(string date = "2026-07-01")
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var gen = await _client.PostAsJsonAsync("/api/tee-time-slots/generate", new
        {
            date, intervalMinutes = 10,
            openTime = "08:00", closeTime = "08:20", maxPlayers = 4
        });
        var slots = await gen.Content.ReadFromJsonAsync<List<SlotDto>>();
        _client.DefaultRequestHeaders.Authorization = null;
        return slots![0].Id;
    }

    [Fact]
    public async Task CreateBooking_ValidSlot_Returns201()
    {
        var slotId = await SeedSlotAsync("2026-07-10");

        var response = await _client.PostAsJsonAsync("/api/bookings", new
        {
            teeTimeSlotId = slotId,
            golferName = "John Doe",
            golferEmail = "john@example.com",
            golferPhone = "780-555-1234",
            numberOfPlayers = 2,
            numberOfCarts = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
        Assert.Equal("John Doe", booking!.GolferName);
        Assert.Equal(2, booking!.NumberOfPlayers);
    }

    [Fact]
    public async Task GetBookings_AsAdmin_ReturnsAllBookings()
    {
        var slotId = await SeedSlotAsync("2026-07-11");
        await _client.PostAsJsonAsync("/api/bookings", new
        {
            teeTimeSlotId = slotId,
            golferName = "Jane Smith",
            golferEmail = "jane@example.com",
            golferPhone = "780-555-5678",
            numberOfPlayers = 1,
            numberOfCarts = 0
        });

        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/bookings?date=2026-07-11");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bookings = await response.Content.ReadFromJsonAsync<List<BookingDto>>();
        Assert.True(bookings!.Count >= 1);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CancelBooking_AsAdmin_Returns204()
    {
        var slotId = await SeedSlotAsync("2026-07-12");
        var create = await _client.PostAsJsonAsync("/api/bookings", new
        {
            teeTimeSlotId = slotId,
            golferName = "Bob Cancel",
            golferEmail = "bob@example.com",
            golferPhone = "780-555-0000",
            numberOfPlayers = 1,
            numberOfCarts = 0
        });
        var booking = await create.Content.ReadFromJsonAsync<BookingDto>();

        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync($"/api/bookings/{booking!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private record SlotDto(Guid Id, string Date, string StartTime,
        int MaxPlayers, bool IsBlocked, string? BlockReason, int BookingCount);
    private record BookingDto(
        Guid Id, Guid TeeTimeSlotId,
        string SlotDate, string SlotTime,
        string GolferName, string GolferEmail, string GolferPhone,
        int NumberOfPlayers, int NumberOfCarts,
        string Status, DateTime BookedAt);
}
