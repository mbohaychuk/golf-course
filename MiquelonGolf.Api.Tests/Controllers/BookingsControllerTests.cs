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

    /// <summary>
    /// Returns a date string N days from today, within the default 14-day booking window.
    /// </summary>
    private static string FutureDate(int daysFromNow = 3) =>
        DateOnly.FromDateTime(DateTime.Today.AddDays(daysFromNow)).ToString("yyyy-MM-dd");

    private async Task<Guid> SeedSlotAsync(string? date = null)
    {
        date ??= FutureDate(3);
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
        // Pick a hole-1 slot so Eighteen/FrontNine round types are valid
        return slots!.First(s => s.StartingHole == 1).Id;
    }

    [Fact]
    public async Task CreateBooking_ValidSlot_Returns201()
    {
        var slotId = await SeedSlotAsync(FutureDate(5));

        var response = await _client.PostAsJsonAsync("/api/bookings", new
        {
            teeTimeSlotId = slotId,
            golferName = "John Doe",
            golferEmail = "john@example.com",
            golferPhone = "780-555-1234",
            numberOfPlayers = 2,
            numberOfCarts = 1,
            roundType = "Eighteen"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var confirmation = await response.Content.ReadFromJsonAsync<BookingConfirmationDto>();
        Assert.NotNull(confirmation!.ConfirmationCode);
        Assert.Equal(2, confirmation.NumberOfPlayers);
    }

    [Fact]
    public async Task GetBookings_AsAdmin_ReturnsAllBookings()
    {
        var date = FutureDate(6);
        var slotId = await SeedSlotAsync(date);
        await _client.PostAsJsonAsync("/api/bookings", new
        {
            teeTimeSlotId = slotId,
            golferName = "Jane Smith",
            golferEmail = "jane@example.com",
            golferPhone = "780-555-5678",
            numberOfPlayers = 1,
            numberOfCarts = 0,
            roundType = "Eighteen"
        });

        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/bookings?date={date}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bookings = await response.Content.ReadFromJsonAsync<List<BookingResponseDto>>();
        Assert.True(bookings!.Count >= 1);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CancelBooking_AsAdmin_Returns204()
    {
        var slotId = await SeedSlotAsync(FutureDate(7));
        var create = await _client.PostAsJsonAsync("/api/bookings", new
        {
            teeTimeSlotId = slotId,
            golferName = "Bob Cancel",
            golferEmail = "bob@example.com",
            golferPhone = "780-555-0000",
            numberOfPlayers = 1,
            numberOfCarts = 0,
            roundType = "Eighteen"
        });
        var confirmation = await create.Content.ReadFromJsonAsync<BookingConfirmationDto>();

        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync($"/api/bookings/{confirmation!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private record SlotDto(Guid Id, string Date, string StartTime,
        int MaxPlayers, bool IsBlocked, string? BlockReason,
        int BookingCount, int StartingHole);

    private record BookingConfirmationDto(
        Guid Id, string ConfirmationCode,
        string SlotDate, string SlotTime, int StartingHole,
        string GolferName, string GolferEmail, string GolferPhone,
        int NumberOfPlayers, int NumberOfCarts,
        string Status, string RoundType);

    private record BookingResponseDto(
        Guid Id, Guid TeeTimeSlotId,
        string SlotDate, string SlotTime, int StartingHole,
        string GolferName, string GolferEmail, string GolferPhone,
        int NumberOfPlayers, int NumberOfCarts,
        string Status, string RoundType, string? ReferralSource,
        string ConfirmationCode, DateTime BookedAt);
}
