using Microsoft.EntityFrameworkCore;
using RealEstateERP.Core.Contract.Service;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Models;
using RealEstateERP.Infrastructure.Context;
using System.Text.Json;
using Xunit;

namespace RealEstateERP.Tests.Infrastructure;

public class AuditTrailTests
{
    private static RealEstateDbContext CreateContext(string dbName, Guid? actor = null)
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new RealEstateDbContext(options, new FakeCurrentUserService(actor));
    }

    private static User NewUser(string username = "audit.user") => new()
    {
        FullName = "Audit User",
        Username = username,
        Email = "audit@agency.et",
        PasswordHash = "hash-not-audited",
        Role = UserRole.Sales,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static JsonElement GetDetails(AuditLog row)
    {
        using var doc = JsonDocument.Parse(row.Details!);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task Create_AppendsOneAuditRow_WithActorAndKeyValues()
    {
        var actor = Guid.NewGuid();
        await using var ctx = CreateContext(nameof(Create_AppendsOneAuditRow_WithActorAndKeyValues), actor);

        var user = NewUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var row = Assert.Single(await ctx.AuditLogs.ToListAsync());
        Assert.Equal("user.created", row.Action);
        Assert.Equal("User", row.EntityType);
        Assert.Equal(user.Id, row.EntityId);
        Assert.Equal(actor, row.ActorUserId);

        var root = GetDetails(row);
        var values = root.GetProperty("values");
        Assert.Equal(user.Username, values.GetProperty("Username").GetString());
        Assert.Equal("Sales", values.GetProperty("Role").GetString());
        Assert.False(root.TryGetProperty("changed_columns", out _));
        // Password hash and timestamps never enter the trail.
        Assert.False(root.ToString().Contains("PasswordHash", StringComparison.OrdinalIgnoreCase));
        Assert.False(root.ToString().Contains("CreatedAt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Modify_RecordsOnlyChangedColumns_WithFromTo()
    {
        var actor = Guid.NewGuid();
        await using var ctx = CreateContext(nameof(Modify_RecordsOnlyChangedColumns_WithFromTo), actor);

        var user = NewUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        var rows = await ctx.AuditLogs.OrderBy(a => a.OccurredAt).ToListAsync();
        Assert.Equal(2, rows.Count); // create + deactivate — UpdatedAt alone adds nothing
        var row = rows[1];
        Assert.Equal("user.deactivated", row.Action);
        Assert.Equal(actor, row.ActorUserId);

        var root = GetDetails(row);
        var changed = root.GetProperty("changed_columns").EnumerateArray().Select(c => c.GetString()).ToList();
        Assert.Equal(new[] { "IsActive" }, changed);
        var values = root.GetProperty("values");
        Assert.True(values.GetProperty("IsActive").GetProperty("from").GetBoolean());
        Assert.False(values.GetProperty("IsActive").GetProperty("to").GetBoolean());
    }

    [Fact]
    public async Task Modify_OnlyIgnoredColumns_ProducesNoAuditRow()
    {
        await using var ctx = CreateContext(nameof(Modify_OnlyIgnoredColumns_ProducesNoAuditRow));

        var user = NewUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        // Routine login updates: last-login + updated-at only.
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        var rows = await ctx.AuditLogs.ToListAsync();
        Assert.Single(rows); // only the create row
    }

    [Fact]
    public async Task PasswordHashChange_IsRedactedToMarker()
    {
        await using var ctx = CreateContext(nameof(PasswordHashChange_IsRedactedToMarker));

        var user = NewUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        user.PasswordHash = "a-different-hash";
        user.UpdatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        var rows = await ctx.AuditLogs.OrderBy(a => a.OccurredAt).ToListAsync();
        var row = rows[1];
        var root = GetDetails(row);
        var values = root.GetProperty("values").GetProperty("PasswordHash");
        Assert.True(values.GetProperty("changed").GetBoolean());
        Assert.False(root.ToString().Contains("a-different-hash", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AuditRows_Themselves_AreNotAudited()
    {
        await using var ctx = CreateContext(nameof(AuditRows_Themselves_AreNotAudited));

        var user = NewUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        user.FullName = "Renamed User";
        await ctx.SaveChangesAsync();

        var rows = await ctx.AuditLogs.ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal("User", r.EntityType));
    }

    [Fact]
    public async Task NoActor_WhenUnauthenticated_AllowsNullActorUserId()
    {
        await using var ctx = CreateContext(nameof(NoActor_WhenUnauthenticated_AllowsNullActorUserId), actor: null);

        var user = NewUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var row = Assert.Single(await ctx.AuditLogs.ToListAsync());
        Assert.Null(row.ActorUserId);
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        private readonly Guid? _userId;

        public FakeCurrentUserService(Guid? userId) => _userId = userId;

        public Guid? UserId => _userId;
    }
}