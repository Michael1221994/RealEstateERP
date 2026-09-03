using Infrastracture.Base.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.Contract.Query;
using RealEstateERP.Core.Features.Auth.Handler.Command;
using RealEstateERP.Core.Features.Auth.Handler.Query;
using RealEstateERP.Core.Models;
using RealEstateERP.Infrastructure.Context;
using Xunit;

namespace RealEstateERP.Tests.Features.Auth;

public class UserManagementHandlerTests
{
    private static RealEstateDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new RealEstateDbContext(options);
    }

    [Fact]
    public async Task CreateUser_HashesPasswordAndPersists()
    {
        await using var ctx = CreateContext(nameof(CreateUser_HashesPasswordAndPersists));
        var handler = new CreateUserCommandHandler(new GenericRepository(ctx), NullLogger<CreateUserCommandHandler>.Instance);

        var result = await handler.Handle(new CreateUserCommand
        {
            FullName = "Hanna Bekele",
            Username = "hanna.finance",
            Email = "hanna@agency.et",
            Password = "Secret123!",
            Role = UserRole.Finance
        }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Success, result.ResponseStatus);
        Assert.NotNull(result.Data);
        Assert.Equal(UserRole.Finance, result.Data.Role);

        var stored = await ctx.Users.SingleAsync(u => u.Username == "hanna.finance");
        Assert.True(BCrypt.Net.BCrypt.Verify("Secret123!", stored.PasswordHash));
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task CreateUser_DuplicateUsername_ReturnsError()
    {
        await using var ctx = CreateContext(nameof(CreateUser_DuplicateUsername_ReturnsError));
        ctx.Users.Add(new User
        {
            FullName = "Existing",
            Username = "dup.user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123!"),
            Role = UserRole.Sales,
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var handler = new CreateUserCommandHandler(new GenericRepository(ctx), NullLogger<CreateUserCommandHandler>.Instance);
        var result = await handler.Handle(new CreateUserCommand
        {
            FullName = "Another",
            Username = "dup.user",
            Password = "Secret123!",
            Role = UserRole.Sales
        }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Error, result.ResponseStatus);
        Assert.Contains("already taken", result.Message);
    }

    [Fact]
    public async Task DeactivateUser_ThenLoginDisabled_IsEnforcedByFlag()
    {
        await using var ctx = CreateContext(nameof(DeactivateUser_ThenLoginDisabled_IsEnforcedByFlag));
        var existing = new User
        {
            FullName = "Sales Person",
            Username = "sales.person",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123!"),
            Role = UserRole.Sales,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(existing);
        await ctx.SaveChangesAsync();

        var setActive = new SetUserActiveCommandHandler(new GenericRepository(ctx), NullLogger<SetUserActiveCommandHandler>.Instance);
        var result = await setActive.Handle(new SetUserActiveCommand { UserId = existing.Id, IsActive = false }, CancellationToken.None);

        Assert.Equal(Infrastracture.Base.ResponseStatus.Success, result.ResponseStatus);
        Assert.False(result.Data!.IsActive);
        Assert.False((await ctx.Users.FindAsync(existing.Id))!.IsActive);
    }

    [Fact]
    public async Task GetUsers_ExcludesInactiveByDefault()
    {
        await using var ctx = CreateContext(nameof(GetUsers_ExcludesInactiveByDefault));
        ctx.Users.AddRange(
            new User { FullName = "Active", Username = "active", PasswordHash = "x", Role = UserRole.Sales, IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { FullName = "Inactive", Username = "inactive", PasswordHash = "x", Role = UserRole.Sales, IsActive = false, CreatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var handler = new GetUsersQueryHandler(new GenericRepository(ctx), NullLogger<GetUsersQueryHandler>.Instance);

        var defaultResult = await handler.Handle(new GetUsersQuery(), CancellationToken.None);
        Assert.Single(defaultResult.Data!);
        Assert.Equal("active", defaultResult.Data![0].Username);

        var withInactive = await handler.Handle(new GetUsersQuery { IncludeInactive = true }, CancellationToken.None);
        Assert.Equal(2, withInactive.Data!.Count);
    }
}
