using CRM.FileStorage.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.FileStorage.Persistence.Configurations;

public abstract class AuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(100);

        builder.Property(e => e.LastModifiedByIp)
            .HasMaxLength(45);

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedBy)
            .HasMaxLength(100);

        builder.Property(e => e.DeletedByIp)
            .HasMaxLength(45);

        builder.Property(e => e.DeletedAt);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}