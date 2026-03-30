// MiquelonGolf.Api/Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<TeeTimeSlot> TeeTimeSlots => Set<TeeTimeSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<GolfEvent> Events => Set<GolfEvent>();
    public DbSet<Hole> Holes => Set<Hole>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<SiteContent> SiteContents => Set<SiteContent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SiteContent>()
            .HasKey(s => s.Key);

        builder.Entity<Hole>()
            .HasIndex(h => h.HoleNumber)
            .IsUnique();

        builder.Entity<Member>()
            .HasOne(m => m.User)
            .WithOne(u => u.Member)
            .HasForeignKey<Member>(m => m.UserId)
            .IsRequired(false);

        builder.Entity<Booking>()
            .HasOne(b => b.TeeTimeSlot)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.TeeTimeSlotId);
    }
}
