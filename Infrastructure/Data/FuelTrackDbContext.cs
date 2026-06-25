using FuelTrack.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelTrack.Api.Infrastructure.Data;

public sealed class FuelTrackDbContext : DbContext
{
    public FuelTrackDbContext(DbContextOptions<FuelTrackDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();
    public DbSet<PaymentHistoryEntity> PaymentHistory => Set<PaymentHistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(32);
            entity.Property(x => x.FullName).HasMaxLength(160);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.PasswordHash).HasMaxLength(255);
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<ProfileEntity>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasMaxLength(32);
            entity.Property(x => x.CompanyName).HasMaxLength(200);
            entity.Property(x => x.Ruc).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.ContactName).HasMaxLength(160);
            entity.Property(x => x.AvatarContentType).HasMaxLength(100);
            entity.HasOne(x => x.User)
                .WithOne(x => x.Profile)
                .HasForeignKey<ProfileEntity>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(32);
            entity.Property(x => x.UserId).HasMaxLength(32);
            entity.Property(x => x.Code).HasMaxLength(40);
            entity.Property(x => x.Product).HasMaxLength(100);
            entity.Property(x => x.Eta).HasMaxLength(200);
            entity.Property(x => x.Plant).HasMaxLength(160);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.TimeWindow).HasMaxLength(100);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.PaymentMethod).HasMaxLength(120);
            entity.Property(x => x.Amount).HasPrecision(14, 2);
            entity.Property(x => x.VehicleId).HasMaxLength(60);
            entity.Property(x => x.VehiclePlate).HasMaxLength(30);
            entity.Property(x => x.DriverName).HasMaxLength(160);
            entity.Property(x => x.LastStatusComment).HasMaxLength(500);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
            entity.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentMethodEntity>(entity =>
        {
            entity.ToTable("payment_methods");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(32);
            entity.Property(x => x.UserId).HasMaxLength(32);
            entity.Property(x => x.Brand).HasMaxLength(40);
            entity.Property(x => x.Last4).HasMaxLength(4);
            entity.Property(x => x.Holder).HasMaxLength(160);
            entity.Property(x => x.Expires).HasMaxLength(5);
            entity.HasOne(x => x.User)
                .WithMany(x => x.PaymentMethods)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentHistoryEntity>(entity =>
        {
            entity.ToTable("payment_history");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(32);
            entity.Property(x => x.UserId).HasMaxLength(32);
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.Property(x => x.Amount).HasPrecision(14, 2);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.User)
                .WithMany(x => x.PaymentHistory)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
