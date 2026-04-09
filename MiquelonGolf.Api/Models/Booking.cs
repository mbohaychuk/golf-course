using System.ComponentModel.DataAnnotations;

namespace MiquelonGolf.Api.Models;

public class Booking
{
    public Guid Id { get; set; }
    public Guid TeeTimeSlotId { get; set; }
    public TeeTimeSlot TeeTimeSlot { get; set; } = null!;
    public DateTime BookedAt { get; set; }
    public string GolferName { get; set; } = string.Empty;
    public string GolferEmail { get; set; } = string.Empty;
    public string GolferPhone { get; set; } = string.Empty;
    public int NumberOfPlayers { get; set; }
    public int NumberOfCarts { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public RoundType RoundType { get; set; } = RoundType.Eighteen;
    public string? ReferralSource { get; set; }
    [MaxLength(8)]
    public string ConfirmationCode { get; set; } = string.Empty;
}
