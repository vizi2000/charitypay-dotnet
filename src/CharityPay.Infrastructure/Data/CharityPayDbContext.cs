using Microsoft.EntityFrameworkCore;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Shared;
using CharityPay.Infrastructure.Data.Configurations;
using System.Reflection;

namespace CharityPay.Infrastructure.Data;

public class CharityPayDbContext : DbContext
{
    public CharityPayDbContext(DbContextOptions<CharityPayDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PaymentEvent> PaymentEvents { get; set; } = null!;
    public DbSet<OrganizationAnalytics> OrganizationAnalytics { get; set; } = null!;
    public DbSet<IoTDevice> IoTDevices { get; set; } = null!;
    public DbSet<DeviceHeartbeat> DeviceHeartbeats { get; set; } = null!;
    public DbSet<DeviceTransaction> DeviceTransactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetAmount).HasPrecision(15, 2);
            entity.Property(e => e.CollectedAmount).HasPrecision(15, 2).HasDefaultValue(0);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(254);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PrimaryColor).HasMaxLength(7);
            entity.Property(e => e.SecondaryColor).HasMaxLength(7);
            entity.Property(e => e.CustomMessage).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasConversion<string>().HasDefaultValue("pending");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithOne(u => u.Organization)
                  .HasForeignKey<Organization>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasConversion<string>().HasDefaultValue("pending");
            entity.Property(e => e.Method).IsRequired().HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.DonorEmail).HasMaxLength(254);
            entity.Property(e => e.DonorName).HasMaxLength(100);
            entity.Property(e => e.FiservOrderId).HasMaxLength(100);
            entity.Property(e => e.FiservCheckoutId).HasMaxLength(100);
            entity.Property(e => e.FiservTransactionId).HasMaxLength(100);
            entity.Property(e => e.ApprovalCode).HasMaxLength(50);
            entity.Property(e => e.RedirectUrl).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.FiservOrderId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasOne(e => e.Organization)
                  .WithMany(o => o.Payments)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DonorEmail).HasMaxLength(254);
            entity.Property(e => e.DonorName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceType).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(3);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EventType, e.Timestamp });
            entity.HasOne(e => e.Payment)
                  .WithMany()
                  .HasForeignKey(e => e.PaymentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganizationAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(15, 2);
            entity.Property(e => e.ConversionRate).HasPrecision(5, 4);
            entity.Property(e => e.AverageDonation).HasPrecision(15, 2);
            entity.Property(e => e.RefundAmount).HasPrecision(15, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.OrganizationId, e.Date }).IsUnique();
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IoTDevice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.FirmwareVersion).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.DeviceId).IsUnique();
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => e.OrganizationId);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceHeartbeat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Temperature).HasPrecision(5, 2);
            entity.Property(e => e.ErrorCode).HasMaxLength(50);
            entity.Property(e => e.Metrics).HasColumnType("jsonb");
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.Device)
                  .WithMany()
                  .HasForeignKey(e => e.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.ApprovalCode).HasMaxLength(50);
            entity.Property(e => e.TerminalId).HasMaxLength(100);
            entity.Property(e => e.MerchantId).HasMaxLength(100);
            entity.Property(e => e.TransactionData).HasColumnType("jsonb");
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.Device)
                  .WithMany()
                  .HasForeignKey(e => e.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Payment)
                  .WithMany()
                  .HasForeignKey(e => e.PaymentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(Entity.CreatedAt)).CurrentValue = DateTimeOffset.UtcNow;
                entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = DateTimeOffset.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = DateTimeOffset.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}