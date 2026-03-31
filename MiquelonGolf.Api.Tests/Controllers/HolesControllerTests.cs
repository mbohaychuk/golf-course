using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class HolesControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public HolesControllerTests(TestWebAppFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetHoles_ReturnsAll18()
    {
        var response = await _client.GetAsync("/api/holes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var holes = await response.Content.ReadFromJsonAsync<List<HoleDto>>();
        Assert.Equal(18, holes!.Count);
    }

    [Fact]
    public async Task GetHole_ByNumber_ReturnsHole()
    {
        var response = await _client.GetAsync("/api/holes/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hole = await response.Content.ReadFromJsonAsync<HoleDto>();
        Assert.Equal(1, hole!.HoleNumber);
    }

    [Fact]
    public async Task UpdateHole_AsAdmin_UpdatesDescription()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync("/api/holes/1", new
        {
            par = 4, handicap = 1,
            yardageBlue = 420, yardageWhite = 390, yardageRed = 340,
            description = "Dogleg left, avoid the trees on the right.",
            imageUrl = (string?)null, diagramUrl = (string?)null
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hole = await response.Content.ReadFromJsonAsync<HoleDto>();
        Assert.Equal("Dogleg left, avoid the trees on the right.", hole!.Description);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private record HoleDto(Guid Id, int HoleNumber, int Par, int Handicap,
        int YardageBlue, int YardageWhite, int YardageRed, string Description,
        string? ImageUrl, string? DiagramUrl);
}
