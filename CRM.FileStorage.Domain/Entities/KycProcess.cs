using CRM.FileStorage.Domain.Common.Entities;
using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Domain.Entities;

public class KycProcess : AuditableEntity
{
    public Guid UserId { get; private set; }
    public KycStatus Status { get; private set; }
    public string SessionToken { get; private set; }
    public DateTimeOffset LastActivityTime { get; private set; }
    public string? VerificationComment { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    private readonly List<StoredFile> _files = new();
    public IReadOnlyCollection<StoredFile> Files => _files.AsReadOnly();

    private KycProcess()
    {
        SessionToken = string.Empty;
        LastActivityTime = DateTimeOffset.UtcNow;
    }

    public KycProcess(Guid userId)
    {
        UserId = userId;
        Status = KycStatus.New;
        SessionToken = GenerateSessionToken();
        LastActivityTime = DateTimeOffset.UtcNow;
    }

    public void UpdateActivity()
    {
        LastActivityTime = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(KycStatus newStatus)
    {
        if (Status == newStatus)
            return;

        if (Status == KycStatus.Verified || Status == KycStatus.Rejected)
            throw new InvalidOperationException("Cannot change status after verification or rejection");

        Status = newStatus;
        LastActivityTime = DateTimeOffset.UtcNow;
    }

    public void AddFile(StoredFile file)
    {
        if (_files.Any(f => f.FileType == file.FileType && f.Status != FileStatus.Deleted))
        {
            throw new InvalidOperationException($"File of type {file.FileType} already exists");
        }

        _files.Add(file);
        file.AssociateWithKycProcess(Id);

        UpdateStatusBasedOnFiles();
        UpdateActivity();
    }

    public void CompleteVerification(bool isApproved, string comment, Guid reviewerId)
    {
        if (Status == KycStatus.Verified || Status == KycStatus.Rejected)
            throw new InvalidOperationException("KYC process already completed");

        Status = isApproved ? KycStatus.Verified : KycStatus.Rejected;
        VerificationComment = comment;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    private void UpdateStatusBasedOnFiles()
    {
        var activeFiles = _files.Where(f => f.Status != FileStatus.Deleted).ToList();

        if (!activeFiles.Any())
        {
            if (Status != KycStatus.New)
                UpdateStatus(KycStatus.New);
            return;
        }

        bool hasIdFront = activeFiles.Any(f => f.FileType == FileType.IdFront);
        bool hasIdBack = activeFiles.Any(f => f.FileType == FileType.IdBack);
        bool hasPassport = activeFiles.Any(f => f.FileType == FileType.PassportMain);
        bool hasFacePhoto = activeFiles.Any(f => f.FileType == FileType.FacePhoto);

        bool hasFullDocuments = (hasIdFront && hasIdBack) || hasPassport;

        if (hasFullDocuments && hasFacePhoto)
        {
            if (Status != KycStatus.DocumentsUploaded && Status != KycStatus.UnderReview
                                                      && Status != KycStatus.Verified && Status != KycStatus.Rejected)
                UpdateStatus(KycStatus.DocumentsUploaded);
        }
        else
        {
            if (Status != KycStatus.PartiallyCompleted && Status != KycStatus.New)
                UpdateStatus(KycStatus.PartiallyCompleted);
            else if (Status == KycStatus.New && activeFiles.Any())
                UpdateStatus(KycStatus.PartiallyCompleted);
        }
    }

    private string GenerateSessionToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 16);
    }
}