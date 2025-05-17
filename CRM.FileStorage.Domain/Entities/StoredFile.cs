using CRM.FileStorage.Domain.Common.Entities;
using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Domain.Entities;

public class StoredFile : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string OriginalFileName { get; private set; }
    public string FileExtension { get; private set; }
    public string ContentType { get; private set; }
    public long FileSize { get; private set; }
    public FileType FileType { get; private set; }
    public FileStatus Status { get; private set; }
    public string FileHash { get; private set; }
    public string StoragePath { get; private set; }
    public string BucketName { get; private set; }
    public Guid? KycProcessId { get; private set; }
    public KycProcess? KycProcess { get; private set; }
    public DateTimeOffset CreationTime { get; private set; }
    public DateTimeOffset? ExpirationTime { get; private set; }

    private StoredFile()
    {
        OriginalFileName = string.Empty;
        FileExtension = string.Empty;
        ContentType = string.Empty;
        FileHash = string.Empty;
        StoragePath = string.Empty;
        BucketName = string.Empty;
        CreationTime = DateTimeOffset.UtcNow;
    }

    public StoredFile(
        Guid userId,
        string originalFileName,
        string fileExtension,
        string contentType,
        long fileSize,
        FileType fileType,
        string fileHash,
        string storagePath,
        string bucketName,
        Guid? kycProcessId = null)
    {
        UserId = userId;
        OriginalFileName = originalFileName;
        FileExtension = fileExtension;
        ContentType = contentType;
        FileSize = fileSize;
        FileType = fileType;
        Status = FileStatus.Temporary;
        FileHash = fileHash;
        StoragePath = storagePath;
        BucketName = bucketName;
        KycProcessId = kycProcessId;
        CreationTime = DateTimeOffset.UtcNow;
        ExpirationTime = DateTimeOffset.UtcNow.AddDays(5);
    }

    public void MakePermanent(string newPath, string newBucket)
    {
        if (Status == FileStatus.Permanent)
            return;

        StoragePath = newPath;
        BucketName = newBucket;
        Status = FileStatus.Permanent;
        ExpirationTime = null;
    }

    public void AssociateWithKycProcess(Guid kycProcessId)
    {
        KycProcessId = kycProcessId;
    }

    public void MarkAsDeleted()
    {
        Status = FileStatus.Deleted;
    }

    public bool IsExpired()
    {
        return Status == FileStatus.Temporary &&
               ExpirationTime.HasValue &&
               ExpirationTime.Value < DateTimeOffset.UtcNow;
    }
}