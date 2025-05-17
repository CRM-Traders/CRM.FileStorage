using CRM.FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.FileStorage.Persistence.Configurations;

public class StoredFileConfiguration : AuditableEntityConfiguration<StoredFile>
{
    public override void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        base.Configure(builder);

        builder.ToTable("StoredFiles");

        builder.Property(f => f.UserId)
            .IsRequired();

        builder.Property(f => f.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.FileExtension)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FileSize)
            .IsRequired();

        builder.Property(f => f.FileType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(f => f.FileHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.BucketName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.KycProcessId);

        builder.Property(f => f.CreationTime)
            .IsRequired();

        builder.Property(f => f.ExpirationTime);

        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.KycProcessId);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.ExpirationTime);
        builder.HasIndex(f => new { f.Status, f.ExpirationTime });

        builder.HasOne(f => f.KycProcess)
            .WithMany(k => k.Files)
            .HasForeignKey(f => f.KycProcessId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}