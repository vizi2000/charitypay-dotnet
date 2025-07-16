using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        
        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(d => d.MimeType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(d => d.FileSize)
            .IsRequired();
            
        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(d => d.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(d => d.UploadedAt)
            .IsRequired();
            
        builder.Property(d => d.VerificationNotes)
            .HasMaxLength(1000);
            
        builder.Property(d => d.CreatedAt)
            .IsRequired();
            
        builder.Property(d => d.UpdatedAt)
            .IsRequired();
            
        // Indexes
        builder.HasIndex(d => d.OrganizationId);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.IsVerified);
        builder.HasIndex(d => d.UploadedAt);
        
        // Relationships
        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Documents)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}