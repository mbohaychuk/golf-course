using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Settings;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/admin/settings")]
public class BookingSettingsController(AppDbContext db) : ControllerBase
{
    private static readonly Dictionary<string, (string Key, int Default, int Min, int Max)> SettingsMap = new()
    {
        ["booking-window"] = ("settings.bookingWindowDays", 14, 1, 60),
        ["turn-time"] = ("settings.turnTimeOffsetMinutes", 135, 60, 240),
        ["tee-time-interval"] = ("settings.teeTimeIntervalMinutes", 10, 6, 20),
        ["max-players"] = ("settings.maxPlayersPerSlot", 4, 1, 8),
    };

    [HttpGet]
    public async Task<ActionResult<BookingSettingsDto>> GetAll()
    {
        var keys = SettingsMap.Values.Select(s => s.Key).ToList();
        var contents = await db.SiteContents
            .Where(sc => keys.Contains(sc.Key))
            .ToDictionaryAsync(sc => sc.Key, sc => sc.Value);

        return Ok(new BookingSettingsDto(
            BookingWindowDays: GetValue(contents, "booking-window"),
            TurnTimeOffsetMinutes: GetValue(contents, "turn-time"),
            TeeTimeIntervalMinutes: GetValue(contents, "tee-time-interval"),
            MaxPlayersPerSlot: GetValue(contents, "max-players")
        ));
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<int>> Get(string name)
    {
        if (!SettingsMap.TryGetValue(name, out var info))
            return NotFound();

        var content = await db.SiteContents.FindAsync(info.Key);
        var value = content != null && int.TryParse(content.Value, out var v) ? v : info.Default;
        return Ok(value);
    }

    [HttpPut("{name}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Set(string name, [FromBody] int value)
    {
        if (!SettingsMap.TryGetValue(name, out var info))
            return NotFound();

        if (value < info.Min || value > info.Max)
            return BadRequest($"Value must be between {info.Min} and {info.Max}.");

        var content = await db.SiteContents.FindAsync(info.Key);
        if (content == null)
        {
            content = new SiteContent
            {
                Key = info.Key,
                Value = value.ToString(),
                LastUpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            db.SiteContents.Add(content);
        }
        else
        {
            content.Value = value.ToString();
            content.LastUpdatedAt = DateTime.UtcNow;
            content.UpdatedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        await db.SaveChangesAsync();
        return Ok(value);
    }

    private int GetValue(Dictionary<string, string> contents, string name)
    {
        var info = SettingsMap[name];
        return contents.TryGetValue(info.Key, out var raw) && int.TryParse(raw, out var v) ? v : info.Default;
    }
}
