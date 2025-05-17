namespace CRM.FileStorage.Domain.Common.Entities;

public abstract class AuditableEntity : Entity
{
    public string CreatedBy { get; private set; } = string.Empty;
    public string? CreatedByIp { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string? LastModifiedBy { get; private set; }
    public string? LastModifiedByIp { get; private set; }
    public DateTimeOffset? LastModifiedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; private set; }
    public string? DeletedByIp { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public void SetCreationTracking(string createdBy, string? ipAddress)
    {
        CreatedBy = createdBy;
        CreatedByIp = ipAddress;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void SetModificationTracking(string modifiedBy, string? ipAddress)
    {
        LastModifiedBy = modifiedBy;
        LastModifiedByIp = ipAddress;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void SetDeletionTracking(string deletedBy, string? ipAddress)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedByIp = ipAddress;
        DeletedAt = DateTimeOffset.UtcNow;
    }

    public void UndoDeletion(string modifiedBy, string? ipAddress)
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedBy = null;
        DeletedByIp = null;
        DeletedAt = null;

        SetModificationTracking(modifiedBy, ipAddress);
    }
}