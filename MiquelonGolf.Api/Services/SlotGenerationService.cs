// MiquelonGolf.Api/Services/SlotGenerationService.cs
using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;

namespace MiquelonGolf.Api.Services;

public class SlotGenerationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlotGenerationService> _logger;

    public SlotGenerationService(IServiceScopeFactory scopeFactory, ILogger<SlotGenerationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay so the DB is fully migrated/seeded before first run
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        await GenerateUpcomingSlots();

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await GenerateUpcomingSlots();
        }
    }

    private async Task GenerateUpcomingSlots()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var teeTimeService = scope.ServiceProvider.GetRequiredService<ITeeTimeService>();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var holidays = await db.CourseHolidays
                .Where(h => h.Date >= today && h.Date <= today.AddDays(6))
                .Select(h => h.Date)
                .ToHashSetAsync();

            var specialHours = await db.SpecialHours
                .Where(s => s.Date >= today && s.Date <= today.AddDays(6))
                .ToListAsync();

            var schedule = await db.OperatingHours.ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);

                // Skip holidays
                if (holidays.Contains(date)) continue;

                // Skip dates that already have slots (preserve manual changes)
                if (await db.TeeTimeSlots.AnyAsync(s => s.Date == date)) continue;

                // Determine open/close times — special hours take priority
                var special = specialHours.FirstOrDefault(s => s.Date == date);
                TimeOnly openTime, closeTime;
                int intervalMinutes, maxPlayers;

                if (special != null)
                {
                    openTime = special.OpenTime;
                    closeTime = special.CloseTime;
                    // Use the regular schedule's interval/maxPlayers for that day of week
                    var dow = (int)date.DayOfWeek;
                    var reg = schedule.FirstOrDefault(s => s.DayOfWeek == dow);
                    intervalMinutes = reg?.IntervalMinutes ?? 10;
                    maxPlayers = reg?.MaxPlayers ?? 4;
                }
                else
                {
                    var dow = (int)date.DayOfWeek;
                    var reg = schedule.FirstOrDefault(s => s.DayOfWeek == dow);
                    if (reg == null || !reg.IsOpen) continue;
                    openTime = reg.OpenTime;
                    closeTime = reg.CloseTime;
                    intervalMinutes = reg.IntervalMinutes;
                    maxPlayers = reg.MaxPlayers;
                }

                var slots = teeTimeService.GenerateSlots(date, intervalMinutes, openTime, closeTime, maxPlayers);
                db.TeeTimeSlots.AddRange(slots);
                _logger.LogInformation("Generated {Count} tee time slots for {Date}", slots.Count, date);
            }

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tee time slots");
        }
    }
}
