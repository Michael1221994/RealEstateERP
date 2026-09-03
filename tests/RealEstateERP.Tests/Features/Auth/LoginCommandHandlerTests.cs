using Infrastracture.Base.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.Handler.Command;
using RealEstateERP.Core.Models;
using RealEstateERP.Infrastructure.Context;
using RealEstateERP.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace RealEstateERP.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private static RealEstateDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new RealEstateDbContext(options);
    }

    private static AccessTokenService CreateTokenService() => new(new JwtOptions
    {
        SecretKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef")),
        Issuer = "RealEstateERP",
        Audience = "realestate-erp-api",
        ExpiryMinutes = 60
    });

    private static async Task<RealEstateDbContext> SeedUserAsync(string dbName, string username, UserRole role, string password, bool isActive = true)
    {
        var ctx = CreateContext(dbName);
        ctx.Users.Add(new User
        {
            FullName = "Test User",
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
        return ctx;
    }

    [Fact]
    public async Task ValidCredentials_ReturnsSuccessWithTokenAndRole()
    {
        await using var ctx = await SeedUserAsync(nameof(ValidCredentials_ReturnsSuccessWithTokenAndRole), "admin", UserRole.Admin, "Secret123!");
        var handler = new LoginCommandHandler(
            new GenericRepository(ctx),
            CreateTokenService(),
            NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(new LoginCommand { Username = "ADMIN", Password = "Secret123!" }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Success, result.ResponseStatus);
        Assert.False(result.IsFailed);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrEmpty(result.Data.Token));
        Assert.Equal(UserRole.Admin, result.Data.User.Role);
        Assert.Equal("admin", result.Data.User.Username);
    }

    [Fact]
    public async Task WrongPassword_ReturnsError()
    {
        await using var ctx = await SeedUserAsync(nameof(WrongPassword_ReturnsError), "finance1", UserRole.Finance, "Secret123!");
        var handler = new LoginCommandHandler(
            new GenericRepository(ctx),
            CreateTokenService(),
            NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(new LoginCommand { Username = "finance1", Password = "WrongPass!" }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Error, result.ResponseStatus);
        Assert.Null(result.Data);
        Assert.Equal("Invalid username or password.", result.Message);
    }

    [Fact]
    public async Task UnknownUsername_ReturnsError()
    {
        await using var ctx = CreateContext(nameof(UnknownUsername_ReturnsError));
        var handler = new LoginCommandHandler(
            new GenericRepository(ctx),
            CreateTokenService(),
            NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(new LoginCommand { Username = "nobody", Password = "Secret123!" }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Error, result.ResponseStatus);
        Assert.Equal("Invalid username or password.", result.Message);
    }

    [Fact]
    public async Task DisabledAccount_ReturnsError()
    {
        await using var ctx = await SeedUserAsync(nameof(DisabledAccount_ReturnsError), "sales1", UserRole.Sales, "Secret123!", isActive: false);
        var handler = new LoginCommandHandler(
            new GenericRepository(ctx),
            CreateTokenService(),
            NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(new LoginCommand { Username = "sales1", Password = "Secret123!" }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Error, result.ResponseStatus);
        Assert.Contains("disabled", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AccessToken_ContainsExpectedClaimsAndValidationData()
    {
        var service = CreateTokenService();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "legal1", FullName = "Legal Officer", Role = UserRole.Legal };

        var token = service.CreateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token.Token);

        Assert.Equal("RealEstateERP", jwt.Issuer);
        Assert.Contains(jwt.Audiences, a => a == "realestate-erp-api");
        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == "UserID").Value);
        Assert.Equal(UserRole.Legal.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }
}
