using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class EventsControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public EventsControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEvents_Public_ReturnsOnlyPublicEvents()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var seed1 = await _client.PostAsJsonAsync("/api/events", new
        {
            title = "Public Tourney", description = "Open to all",
            eventDate = "2026-08-01", isPublic = true, category = "Tournament"
        });
        Assert.Equal(HttpStatusCode.Created, seed1.StatusCode);
        var seed2 = await _client.PostAsJsonAsync("/api/events", new
        {
            title = "Members Night", description = "Members only",
            eventDate = "2026-08-02", isPublic = false, category = "SocialNight"
        });
        Assert.Equal(HttpStatusCode.Created, seed2.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();
        Assert.All(events!, e => Assert.True(e.IsPublic));
    }

    [Fact]
    public async Task GetEvents_AsAdmin_ReturnsAllEvents()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/events", new
        {
            title = "Admin Only Event", description = "Test",
            eventDate = "2026-09-01", isPublic = false, category = "Tournament"
        });

        var response = await _client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();
        Assert.True(events!.Count >= 1);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CreateEvent_AsAdmin_Returns201()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            title = "Ladies Night", description = "Weekly ladies golf",
            eventDate = "2026-06-10", startTime = "18:30",
            isPublic = false, category = "LadiesNight"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var ev = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.Equal("Ladies Night", ev!.Title);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private record EventDto(Guid Id, string Title, string Description,
        string EventDate, string? StartTime, bool IsPublic, string Category, string? ImageUrl);
}
