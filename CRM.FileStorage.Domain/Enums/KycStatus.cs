namespace CRM.FileStorage.Domain.Enums;

public enum KycStatus
{
    New = 1,
    PartiallyCompleted = 2,
    DocumentsUploaded = 3,
    UnderReview = 4,
    Verified = 5,
    Rejected = 6
}