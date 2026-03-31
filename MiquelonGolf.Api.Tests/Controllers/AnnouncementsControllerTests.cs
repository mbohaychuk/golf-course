using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class AnnouncementsControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public AnnouncementsControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActive_ReturnsOnlyActiveAnnouncements()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var s1 = await _client.PostAsJsonAsync("/api/announcements", new
        {
            message = "Course open — great conditions",
            isActive = true, type = "CourseConditions"
        });
        Assert.Equal(HttpStatusCode.Created, s1.StatusCode);
        var s2 = await _client.PostAsJsonAsync("/api/announcements", new
        {
            message = "Old notice", isActive = false, type = "General"
        });
        Assert.Equal(HttpStatusCode.Created, s2.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/announcements/active");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var announcements = await response.Content
            .ReadFromJsonAsync<List<AnnouncementDto>>();
        Assert.All(announcements!, a => Assert.True(a.IsActive));
    }

    [Fact]
    public async Task CreateAnnouncement_AsAdmin_Returns201()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/announcements", new
        {
            message = "Cart paths only today — wet fairways",
            isActive = true, type = "CourseConditions",
            expiresAt = (string?)null
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var a = await response.Content.ReadFromJsonAsync<AnnouncementDto>();
        Assert.Contains("Cart paths", a!.Message);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private record AnnouncementDto(Guid Id, string Message,
        bool IsActive, string Type, DateTime CreatedAt, DateTime? ExpiresAt);
}
