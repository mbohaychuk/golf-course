// MiquelonGolf.Api/Controllers/SiteContentController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.SiteContent;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/site-content")]
public class SiteContentController : ControllerBase
{
    private readonly AppDbContext _db;
    public SiteContentController(AppDbContext db) => _db = db;

    // Keys with these prefixes are safe to read anonymously — they back
    // public-facing pages (fees, hours). Settings (settings.*) and any future
    // admin-only content stay behind the Admin GetAll endpoint.
    private static readonly string[] PublicKeyPrefixes = ["fees.", "hours."];

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var content = await _db.SiteContents.FindAsync(key);
        if (content == null) return NotFound();
        return Ok(new SiteContentResponse(content.Key, content.Value, content.LastUpdatedAt));
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic()
    {
        var all = await _db.SiteContents
            .AsNoTracking()
            .OrderBy(c => c.Key)
            .ToListAsync();
        var publicEntries = all
            .Where(c => PublicKeyPrefixes.Any(p => c.Key.StartsWith(p)))
            .Select(c => new SiteContentResponse(c.Key, c.Value, c.LastUpdatedAt));
        return Ok(publicEntries);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var all = await _db.SiteContents.OrderBy(c => c.Key).ToListAsync();
        return Ok(all.Select(c =>
            new SiteContentResponse(c.Key, c.Value, c.LastUpdatedAt)));
    }

    [HttpPut("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Set(string key, [FromBody] SetSiteContentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var existing = await _db.SiteContents.FindAsync(key);
        var now = DateTime.UtcNow;

        if (existing != null)
        {
            existing.Value = request.Value;
            existing.LastUpdatedAt = now;
            existing.UpdatedByUserId = userId;
        }
        else
        {
            _db.SiteContents.Add(new SiteContent
            {
                Key = key,
                Value = request.Value,
                LastUpdatedAt = now,
                UpdatedByUserId = userId
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new SiteContentResponse(key, request.Value, now));
    }
}
