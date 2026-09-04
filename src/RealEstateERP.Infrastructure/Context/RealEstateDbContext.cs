using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using RealEstateERP.Core.Contract.Service;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Infrastructure.Context;

public class RealEstateDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options, ICurrentUserService? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(200);
            entity.Property(u => u.Phone).HasMaxLength(30);
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(u => u.IsActive).IsRequired();
            entity.Property(u => u.LastLoginAt);
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.UpdatedAt);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Details).HasColumnType("jsonb");
            entity.Property(a => a.OccurredAt).IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Queries on the trail: "everything for entity X" and retention sweeps by date.
            entity.HasIndex(a => new { a.EntityType, a.EntityId, a.OccurredAt });
            entity.HasIndex(a => a.OccurredAt);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CaptureAuditTrail();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        CaptureAuditTrail();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // --- Audit trail (F-AUTH-04) -----------------------------------------------------------
    //
    // Every Added/Modified/Deleted non-audit entity gets one audit_logs row written in the
    // SAME SaveChanges call (same transaction). Details carry only the changed columns
    // (old → new), never full row snapshots. Timestamps and last-login are excluded so
    // routine logins don't spam the trail; password hashes are redacted to a "changed"
    // marker. Actor comes from the token claim (ICurrentUserService) — never a body value.

    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(User.CreatedAt),
        nameof(User.UpdatedAt),
        nameof(User.LastLoginAt)
    };

    private const string PasswordHashProperty = nameof(User.PasswordHash);

    private void CaptureAuditTrail()
    {
        ChangeTracker.DetectChanges();

        var now = DateTime.UtcNow;
        var actorId = _currentUser?.UserId;

        // Snapshot first so the audit rows we add below are not themselves audited.
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog
                        && (e.State == EntityState.Added
                            || e.State == EntityState.Modified
                            || e.State == EntityState.Deleted))
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var details = BuildDetails(entry);
            if (details is null)
            {
                // Only ignored/unchanged properties were touched (e.g. routine login) — no row.
                continue;
            }

            AuditLogs.Add(new AuditLog
            {
                ActorUserId = actorId,
                Action = GetAction(entry, entityType),
                EntityType = entityType,
                EntityId = GetEntityId(entry),
                Details = details,
                OccurredAt = now
            });
        }
    }

    private static string GetAction(EntityEntry entry, string entityType)
    {
        var name = entityType.ToLowerInvariant();
        return entry.State switch
        {
            EntityState.Added => $"{name}.created",
            EntityState.Deleted => $"{name}.deleted",
            _ => IsActiveToggled(entry)
                ? (bool)entry.Property(nameof(User.IsActive)).CurrentValue! ? $"{name}.activated" : $"{name}.deactivated"
                : $"{name}.updated"
        };
    }

    private static bool IsActiveToggled(EntityEntry entry)
    {
        var property = entry.Metadata.FindProperty(nameof(User.IsActive));
        if (property is null)
        {
            return false;
        }
        var current = entry.Property(nameof(User.IsActive)).CurrentValue;
        var original = entry.Property(nameof(User.IsActive)).OriginalValue;
        return current is bool && original is bool && !Equals(current, original);
    }

    private static Guid? GetEntityId(EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties ?? Array.Empty<IProperty>();
        foreach (var property in keyProperties)
        {
            if (entry.Property(property.Name).CurrentValue is Guid id)
            {
                return id;
            }
        }
        return null;
    }

    private static string? BuildDetails(EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();
        var changedColumns = new List<string>();

        switch (entry.State)
        {
            case EntityState.Added:
            case EntityState.Deleted:
                // Snapshot the non-ignored, non-empty fields — enough to identify the row
                // without copying whole rows into the trail.
                foreach (var property in entry.Properties)
                {
                    if (ShouldIgnore(property) || property.Metadata.Name == PasswordHashProperty)
                    {
                        continue;
                    }
                    var value = property.CurrentValue;
                    if (value is null || IsEmptyish(value))
                    {
                        continue;
                    }
                    values[property.Metadata.Name] = Normalize(value);
                }
                break;

            case EntityState.Modified:
                foreach (var property in entry.Properties)
                {
                    if (ShouldIgnore(property))
                    {
                        continue;
                    }
                    var original = property.OriginalValue;
                    var current = property.CurrentValue;
                    if (Equals(original, current))
                    {
                        continue;
                    }

                    changedColumns.Add(property.Metadata.Name);
                    if (property.Metadata.Name == PasswordHashProperty)
                    {
                        // Never write hash values into the trail — a marker only.
                        values[property.Metadata.Name] = new { changed = true };
                    }
                    else
                    {
                        values[property.Metadata.Name] = new { from = Normalize(original), to = Normalize(current) };
                    }
                }
                break;
        }

        if (values.Count == 0)
        {
            return null; // Only ignored/unchanged properties touched (e.g. routine login) — no noise.
        }

        var details = new Dictionary<string, object?>
        {
            ["values"] = values
        };
        if (entry.State == EntityState.Modified)
        {
            details["changed_columns"] = changedColumns;
        }

        return JsonSerializer.Serialize(details);
    }

    private static bool ShouldIgnore(PropertyEntry property) => IgnoredProperties.Contains(property.Metadata.Name);

    private static bool IsEmptyish(object value) => value is string s && string.IsNullOrWhiteSpace(s);

    private static object? Normalize(object? value) => value switch
    {
        null => null,
        Enum e => e.ToString(),
        Guid g => g.ToString(),
        DateTime dt => dt.ToUniversalTime().ToString("O"),
        _ => value
    };
}