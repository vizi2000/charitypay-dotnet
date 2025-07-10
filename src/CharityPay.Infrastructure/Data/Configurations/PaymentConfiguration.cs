using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        
        builder.Property(p => p.Amount)
            .HasPrecision(15, 2);
            
        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue("pending");
            
        builder.Property(p => p.Method)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();
            
        builder.Property(p => p.DonorEmail)
            .HasMaxLength(254);
            
        builder.Property(p => p.DonorName)
            .HasMaxLength(100);
            
        builder.Property(p => p.FiservOrderId)
            .HasMaxLength(100);
            
        builder.Property(p => p.FiservCheckoutId)
            .HasMaxLength(100);
            
        builder.Property(p => p.FiservTransactionId)
            .HasMaxLength(100);
            
        builder.Property(p => p.ApprovalCode)
            .HasMaxLength(50);
            
        builder.Property(p => p.RedirectUrl)
            .HasMaxLength(1000);
            
        builder.Property(p => p.CreatedAt)
            .IsRequired();
            
        builder.Property(p => p.UpdatedAt)
            .IsRequired();
            
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OrganizationId);
        builder.HasIndex(p => p.FiservOrderId);
        builder.HasIndex(p => p.CreatedAt);
        
        builder.HasOne(p => p.Organization)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}