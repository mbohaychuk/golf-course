// MiquelonGolf.Api/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using MiquelonGolf.Api.Data;
using MiquelonGolf.Api.Models;
using MiquelonGolf.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be configured and at least 32 characters long.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.UseSecurityTokenValidators = true; // Use JwtSecurityTokenHandler (compatible with IdentityModel 8.x)
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITeeTimeService, TeeTimeService>();
builder.Services.AddHostedService<SlotGenerationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var originsConfig = builder.Configuration["Cors:AllowedOrigins"];
        if (string.IsNullOrWhiteSpace(originsConfig))
        {
            if (!builder.Environment.IsDevelopment())
                throw new InvalidOperationException("Cors:AllowedOrigins must be configured in production.");
            originsConfig = "http://localhost:3000";
        }
        var origins = originsConfig.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles, admin user, and 18 holes on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure roles exist
    foreach (var role in new[] { "Admin", "Public" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed default admin user if none exists
    const string adminEmail = "admin@miquelonhills.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };
        var adminPassword = builder.Configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Seed:AdminPassword must be configured.");
        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed default operating hours (7 days) if not present
    if (!await db.OperatingHours.AnyAsync())
    {
        var defaultHours = Enumerable.Range(0, 7).Select(dow => new MiquelonGolf.Api.Models.OperatingHours
        {
            DayOfWeek = dow,
            IsOpen = dow != 1, // Closed Mondays by default
            OpenTime = new TimeOnly(7, 0),
            CloseTime = new TimeOnly(19, 0),
            IntervalMinutes = 10,
            MaxPlayers = 4
        });
        db.OperatingHours.AddRange(defaultHours);
        await db.SaveChangesAsync();
    }

    // Seed 18 holes if not present
    if (!await db.Holes.AnyAsync())
    {
        var holes = Enumerable.Range(1, 18).Select(n => new Hole
        {
            Id = Guid.NewGuid(),
            HoleNumber = n,
            Par = n % 3 == 0 ? 5 : n % 3 == 1 ? 4 : 3,
            Handicap = n,
            Description = string.Empty
        });
        db.Holes.AddRange(holes);
        await db.SaveChangesAsync();
    }

    // Seed booking settings defaults
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    var settingsDefaults = new Dictionary<string, string>
    {
        ["settings.bookingWindowDays"] = "14",
        ["settings.turnTimeOffsetMinutes"] = "135",
        ["settings.teeTimeIntervalMinutes"] = "10",
        ["settings.maxPlayersPerSlot"] = "4",
    };
    foreach (var (key, defaultValue) in settingsDefaults)
    {
        if (!await db.SiteContents.AnyAsync(sc => sc.Key == key))
        {
            db.SiteContents.Add(new SiteContent
            {
                Key = key,
                Value = defaultValue,
                LastUpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = adminUser?.Id ?? ""
            });
        }
    }
    await db.SaveChangesAsync();
}

app.Run();

public partial class Program { } // Required for WebApplicationFactory in tests
