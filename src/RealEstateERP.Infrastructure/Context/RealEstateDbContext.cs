using Microsoft.EntityFrameworkCore;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Infrastructure.Context;

public class RealEstateDbContext : DbContext
{
    public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

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

        base.OnModelCreating(modelBuilder);
    }
}
