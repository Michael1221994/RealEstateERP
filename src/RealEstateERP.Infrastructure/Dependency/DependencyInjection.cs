using Infrastracture.Base;
using Infrastracture.Base.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateERP.Core.Features.Auth.Contract.Service;
using RealEstateERP.Infrastructure.Context;
using RealEstateERP.Infrastructure.Services;

namespace RealEstateERP.Infrastructure.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(
                "DefaultConnection",
                "ConnectionStrings:DefaultConnection is not configured.");
        }

        // Jwt section — validated eagerly so misconfiguration fails at startup, not at first login.
        var jwtOptions = new JwtOptions
        {
            SecretKey = configuration["Jwt:SecretKey"] ?? string.Empty,
            Issuer = configuration["Jwt:Issuer"] ?? "RealEstateERP",
            Audience = configuration["Jwt:Audience"] ?? "realestate-erp-api",
            ExpiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var minutes) && minutes > 0
                ? minutes
                : 480
        };

        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
        {
            throw new ArgumentNullException("Jwt:SecretKey", "Jwt:SecretKey is not configured.");
        }

        try
        {
            _ = Convert.FromBase64String(jwtOptions.SecretKey);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Jwt:SecretKey must be a base64-encoded key.", "Jwt:SecretKey");
        }

        services.AddSingleton(jwtOptions);

        services.AddDbContext<RealEstateDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddMemoryCache();

        services.AddScoped<IRepository>(sp =>
            new GenericRepository(sp.GetRequiredService<RealEstateDbContext>()));

        services.AddScoped<IAccessTokenService, AccessTokenService>();

        return services;
    }
}
