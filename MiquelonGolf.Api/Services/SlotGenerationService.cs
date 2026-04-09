using Microsoft.EntityFrameworkCore;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.Services;

namespace MiquelonGolf.Api.Services;

public class SlotGenerationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SlotGenerationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        await GenerateUpcomingSlots(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await GenerateUpcomingSlots(stoppingToken);
        }
    }

    private async Task GenerateUpcomingSlots(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var teeTimeService = scope.ServiceProvider.GetRequiredService<ITeeTimeService>();

            // Read booking window from settings (default 14 days)
            var windowContent = await db.SiteContents.FindAsync(
                new object[] { "settings.bookingWindowDays" }, ct);
            var windowDays = windowContent != null && int.TryParse(windowContent.Value, out var wd) ? wd : 14;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var holidays = (await db.CourseHolidays
                .Where(h => h.Date >= today && h.Date <= today.AddDays(windowDays))
                .ToListAsync(ct))
                .Select(h => h.Date)
                .ToHashSet();

            var specialHours = await db.SpecialHours
                .Where(sh => sh.Date >= today && sh.Date <= today.AddDays(windowDays))
                .ToListAsync(ct);

            var operatingHours = await db.OperatingHours.ToListAsync(ct);

            // Read default interval and max players from settings
            var intervalContent = await db.SiteContents.FindAsync(
                new object[] { "settings.teeTimeIntervalMinutes" }, ct);
            var defaultInterval = intervalContent != null && int.TryParse(intervalContent.Value, out var iv) ? iv : 10;

            var maxPlayersContent = await db.SiteContents.FindAsync(
                new object[] { "settings.maxPlayersPerSlot" }, ct);
            var defaultMaxPlayers = maxPlayersContent != null && int.TryParse(maxPlayersContent.Value, out var mp) ? mp : 4;

            for (var i = 0; i <= windowDays; i++)
            {
                var date = today.AddDays(i);

                if (holidays.Contains(date)) continue;

                // Skip if ANY slots exist for this date (preserves manual edits)
                var hasSlots = await db.TeeTimeSlots.AnyAsync(s => s.Date == date, ct);
                if (hasSlots) continue;

                var special = specialHours.FirstOrDefault(sh => sh.Date == date);
                var daySchedule = operatingHours.FirstOrDefault(oh => oh.DayOfWeek == (int)date.DayOfWeek);

                TimeOnly openTime, closeTime;
                int interval, maxPlayers;

                if (special != null)
                {
                    openTime = special.OpenTime;
                    closeTime = special.CloseTime;
                    interval = daySchedule?.IntervalMinutes ?? defaultInterval;
                    maxPlayers = daySchedule?.MaxPlayers ?? defaultMaxPlayers;
                }
                else if (daySchedule != null && daySchedule.IsOpen)
                {
                    openTime = daySchedule.OpenTime;
                    closeTime = daySchedule.CloseTime;
                    interval = daySchedule.IntervalMinutes;
                    maxPlayers = daySchedule.MaxPlayers;
                }
                else
                {
                    continue; // Closed day
                }

                // Generate for Hole 1
                var hole1Slots = teeTimeService.GenerateSlots(date, interval, openTime, closeTime, maxPlayers, 1);
                db.TeeTimeSlots.AddRange(hole1Slots);

                // Generate for Hole 10
                var hole10Slots = teeTimeService.GenerateSlots(date, interval, openTime, closeTime, maxPlayers, 10);
                db.TeeTimeSlots.AddRange(hole10Slots);

                logger.LogInformation(
                    "Generated {Hole1Count} Hole 1 + {Hole10Count} Hole 10 slots for {Date}",
                    hole1Slots.Count, hole10Slots.Count, date);
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating upcoming tee time slots");
        }
    }
}
