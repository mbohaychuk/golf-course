using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class SiteContentControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public SiteContentControllerTests(TestWebAppFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task SetContent_AsAdmin_StoresValue()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync("/api/site-content/rv.description",
            new { value = "Stay and play at Miquelon Hills." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetContent_ByKey_ReturnsValue()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await _client.PutAsJsonAsync("/api/site-content/hours.monday",
            new { value = "08:00 AM - 07:00 PM" });

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/site-content/hours.monday");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ContentDto>();
        Assert.Equal("08:00 AM - 07:00 PM", content!.Value);
    }

    [Fact]
    public async Task SetContent_AsPublic_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PutAsJsonAsync("/api/site-content/fees.adult",
            new { value = "675.00" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record ContentDto(string Key, string Value, DateTime LastUpdatedAt);
}
