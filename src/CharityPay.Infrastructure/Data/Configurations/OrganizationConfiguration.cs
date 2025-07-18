using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(o => o.Description)
            .IsRequired();
            
        builder.Property(o => o.Category)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(o => o.Location)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(o => o.TargetAmount)
            .HasPrecision(15, 2);
            
        builder.Property(o => o.CollectedAmount)
            .HasPrecision(15, 2)
            .HasDefaultValue(0);
            
        builder.Property(o => o.ContactEmail)
            .IsRequired()
            .HasMaxLength(254);
            
        builder.Property(o => o.Website)
            .HasMaxLength(500);
            
        builder.Property(o => o.Phone)
            .HasMaxLength(20);
            
        builder.Property(o => o.Address)
            .HasMaxLength(300);
            
        builder.Property(o => o.LogoUrl)
            .HasMaxLength(500);
            
        builder.Property(o => o.PrimaryColor)
            .HasMaxLength(7);
            
        builder.Property(o => o.SecondaryColor)
            .HasMaxLength(7);
            
        builder.Property(o => o.CustomMessage)
            .HasMaxLength(1000);
            
        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(OrganizationStatus.Pending);
            
        // Merchant-specific fields
        builder.Property(o => o.LegalBusinessName)
            .HasMaxLength(200);
            
        builder.Property(o => o.KrsNumber)
            .HasMaxLength(20);
            
        builder.Property(o => o.PolcardMerchantId)
            .HasMaxLength(100);
            
        // Value object configurations
        builder.Property(o => o.TaxId)
            .HasConversion(
                nip => nip != null ? nip.Value : null,
                value => value != null ? new Domain.ValueObjects.Nip(value) : null)
            .HasMaxLength(10);
            
        builder.Property(o => o.BankAccount)
            .HasConversion(
                account => account != null ? account.Iban : null,
                value => value != null ? new Domain.ValueObjects.BankAccount(value) : null)
            .HasMaxLength(28);
            
        builder.Property(o => o.CreatedAt)
            .IsRequired();
            
        builder.Property(o => o.UpdatedAt)
            .IsRequired();
            
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.UserId).IsUnique();
        builder.HasIndex(o => o.PolcardMerchantId).IsUnique();
        builder.HasIndex(o => o.TaxId).IsUnique();
        
        builder.HasOne(o => o.User)
            .WithOne(u => u.Organization)
            .HasForeignKey<Organization>(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(o => o.Documents)
            .WithOne(d => d.Organization)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}