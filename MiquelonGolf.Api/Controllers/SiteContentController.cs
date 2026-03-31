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

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var content = await _db.SiteContents.FindAsync(key);
        if (content == null) return NotFound();
        return Ok(new SiteContentResponse(content.Key, content.Value, content.LastUpdatedAt));
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

        if (existing != null)
        {
            existing.Value = request.Value;
            existing.LastUpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;
        }
        else
        {
            _db.SiteContents.Add(new SiteContent
            {
                Key = key,
                Value = request.Value,
                LastUpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = userId
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new SiteContentResponse(key, request.Value, DateTime.UtcNow));
    }
}
