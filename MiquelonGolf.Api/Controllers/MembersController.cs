// MiquelonGolf.Api/Controllers/MembersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.DTOs.Members;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Controllers;

[ApiController]
[Route("api/members")]
[Authorize(Roles = "Admin")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;
    public MembersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var members = await _db.Members.OrderBy(m => m.LastName).ToListAsync();
        return Ok(members.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        return Ok(ToResponse(member));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        if (!Enum.TryParse<MembershipType>(request.MembershipType, out var type))
            return BadRequest("Invalid MembershipType.");
        if (!DateOnly.TryParse(request.MemberSince, out var memberSince))
            return BadRequest("Invalid memberSince date.");
        if (!DateOnly.TryParse(request.PurchaseDate, out var purchaseDate))
            return BadRequest("Invalid purchaseDate.");
        if (!DateOnly.TryParse(request.ExpiryDate, out var expiryDate))
            return BadRequest("Invalid expiryDate.");

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            MembershipType = type,
            MemberSince = memberSince,
            SeasonYear = request.SeasonYear,
            PurchaseDate = purchaseDate,
            ExpiryDate = expiryDate,
            CartTrackage = request.CartTrackage,
            SeasonalCartRental = request.SeasonalCartRental
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, ToResponse(member));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        if (!Enum.TryParse<MembershipType>(request.MembershipType, out var type))
            return BadRequest("Invalid MembershipType.");
        if (!DateOnly.TryParse(request.PurchaseDate, out var purchaseDate))
            return BadRequest("Invalid purchaseDate.");
        if (!DateOnly.TryParse(request.ExpiryDate, out var expiryDate))
            return BadRequest("Invalid expiryDate.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.MembershipType = type;
        member.SeasonYear = request.SeasonYear;
        member.PurchaseDate = purchaseDate;
        member.ExpiryDate = expiryDate;
        member.CartTrackage = request.CartTrackage;
        member.SeasonalCartRental = request.SeasonalCartRental;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(member));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static MemberResponse ToResponse(Member m) =>
        new(m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
            m.MembershipType.ToString(), m.MemberSince.ToString("yyyy-MM-dd"),
            m.SeasonYear, m.PurchaseDate.ToString("yyyy-MM-dd"),
            m.ExpiryDate.ToString("yyyy-MM-dd"), m.CartTrackage, m.SeasonalCartRental);
}
