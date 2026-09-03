using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Models;
using RealEstateERP.Infrastructure.Context;

namespace RealEstateERP.Infrastructure.Seed;

public static class DbSeeder
{
    /// <summary>
    /// Creates the initial administrator if no admin exists yet. Idempotent and safe to run on
    /// every startup. Credentials come from the Seed configuration section (never committed
    /// secrets; override in production).
    /// </summary>
    public static async Task SeedAdminAsync(
        RealEstateDbContext db,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin, ct))
        {
            return;
        }

        var username = (configuration["Seed:AdminUsername"] ?? "admin").Trim().ToLowerInvariant();
        var password = configuration["Seed:AdminPassword"];
        var fullName = configuration["Seed:AdminFullName"] ?? "System Administrator";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Seed:AdminUsername and Seed:AdminPassword must be configured to create the initial administrator.");
        }

        db.Users.Add(new User
        {
            FullName = fullName,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }
}
