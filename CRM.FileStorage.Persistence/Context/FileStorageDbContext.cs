using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Domain.Common.Entities;
using CRM.FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.FileStorage.Persistence.Context;

public class FileStorageDbContext(
    DbContextOptions<FileStorageDbContext> options,
    ICurrentUserContext? currentUserContext = null)
    : DbContext(options)
{
    public DbSet<StoredFile> Files => Set<StoredFile>();
    public DbSet<KycProcess> KycProcesses => Set<KycProcess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileStorageDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    private void ApplyAuditInformation()
    {
        var userId = currentUserContext?.UserId?.ToString() ?? "System";
        var userIp = currentUserContext?.IpAddress;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreationTracking(userId, userIp);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetModificationTracking(userId, userIp);

                    if (entry.Properties.Any(p => p.Metadata.Name == nameof(AuditableEntity.IsDeleted)) &&
                        entry.Property(nameof(AuditableEntity.IsDeleted)).CurrentValue is true &&
                        entry.Property(nameof(AuditableEntity.IsDeleted)).OriginalValue is false)
                    {
                        entry.Entity.SetDeletionTracking(userId, userIp);
                    }

                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SetDeletionTracking(userId, userIp);
                    break;
            }
        }
    }
}