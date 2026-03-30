// MiquelonGolf.Api.Tests/Controllers/MembersControllerTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiquelonGolf.Api.Tests.Helpers;
using Xunit;

namespace MiquelonGolf.Api.Tests.Controllers;

public class MembersControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public MembersControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    private async Task SetAdminAuthAsync()
    {
        var token = await AuthHelpers.GetAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateMember_AsAdmin_Returns201()
    {
        await SetAdminAuthAsync();
        var response = await _client.PostAsJsonAsync("/api/members", new
        {
            firstName = "Randy",
            lastName = "Golfer",
            email = "randy@test.com",
            phone = "780-555-0001",
            membershipType = "Adult",
            memberSince = "2015-01-01",
            seasonYear = 2026,
            purchaseDate = "2026-04-01",
            expiryDate = "2026-10-31",
            cartTrackage = false,
            seasonalCartRental = false
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        Assert.Equal("Randy", member!.FirstName);
    }

    [Fact]
    public async Task GetMembers_AsAdmin_ReturnsAll()
    {
        await SetAdminAuthAsync();
        await _client.PostAsJsonAsync("/api/members", new
        {
            firstName = "List", lastName = "Test", email = "list@test.com",
            membershipType = "Senior", memberSince = "2010-01-01",
            seasonYear = 2026, purchaseDate = "2026-04-01",
            expiryDate = "2026-10-31", cartTrackage = false, seasonalCartRental = false
        });

        var response = await _client.GetAsync("/api/members");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<MemberDto>>();
        Assert.True(members!.Count >= 1);
    }

    [Fact]
    public async Task GetMembers_AsPublic_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/members");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record MemberDto(Guid Id, string FirstName, string LastName,
        string? Email, string? Phone, string MembershipType,
        string MemberSince, int SeasonYear,
        string PurchaseDate, string ExpiryDate,
        bool CartTrackage, bool SeasonalCartRental);
}
