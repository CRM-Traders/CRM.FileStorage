using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Common.Models;

public class KycProcessDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public KycStatus Status { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActivityTime { get; set; }
    public string? VerificationComment { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public IEnumerable<StoredFileDto> Files { get; set; } = new List<StoredFileDto>();
    
    public bool HasIdFront => Files.Any(f => f.FileType == FileType.IdFront);
    public bool HasIdBack => Files.Any(f => f.FileType == FileType.IdBack);
    public bool HasPassport => Files.Any(f => f.FileType == FileType.PassportMain);
    public bool HasFacePhoto => Files.Any(f => f.FileType == FileType.FacePhoto);
    
    public bool IsDocumentationComplete => 
        ((HasIdFront && HasIdBack) || HasPassport) && HasFacePhoto;
}