// MiquelonGolf.Api.Tests/TestWebAppFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.Models;

namespace MiquelonGolf.Api.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related registrations to avoid dual-provider conflict
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || d.ServiceType == typeof(AppDbContext)
                    || (d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition().FullName ==
                        "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1" &&
                        d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext)))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            // Replace with SQLite in-memory
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            // Create schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // Seed Identity roles
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in new[] { "Admin", "Member", "Public" })
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }

            // Seed Admin user
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();
            const string adminEmail = "admin@test.com";
            const string adminPassword = "Admin1234!";
            if (userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User"
                };
                userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
                userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
            }
        });

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "CHANGE-THIS-TO-A-LONG-RANDOM-SECRET-KEY-IN-PRODUCTION",
                ["Jwt:Issuer"] = "MiquelonGolfApi",
                ["Jwt:Audience"] = "MiquelonGolfClient",
                ["Jwt:ExpiryMinutes"] = "1440"
            });
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Member", "Public" })
        {
            if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
        }

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        const string adminEmail = "admin@test.com";
        const string adminPassword = "Admin1234!";
        if (userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User"
            };
            userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
