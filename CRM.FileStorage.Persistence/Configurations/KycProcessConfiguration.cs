using CRM.FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.FileStorage.Persistence.Configurations;

public class KycProcessConfiguration : AuditableEntityConfiguration<KycProcess>
{
    public override void Configure(EntityTypeBuilder<KycProcess> builder)
    {
        base.Configure(builder);

        builder.ToTable("KycProcesses");

        builder.Property(k => k.UserId)
            .IsRequired();

        builder.Property(k => k.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(k => k.SessionToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.LastActivityTime)
            .IsRequired();

        builder.Property(k => k.VerificationComment)
            .HasMaxLength(1000);

        builder.Property(k => k.ReviewedBy);

        builder.Property(k => k.ReviewedAt);

        builder.HasIndex(k => k.UserId);
        builder.HasIndex(k => k.Status);
        builder.HasIndex(k => k.SessionToken)
            .IsUnique();
        builder.HasIndex(k => k.LastActivityTime);
    }
}