using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Common.Models;

public class VerifyKycProcessRequest
{
    public Guid KycProcessId { get; set; }
    public bool IsApproved { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class VerifyKycProcessResponse
{
    public Guid KycProcessId { get; set; }
    public KycStatus Status { get; set; }
    public string? Comment { get; set; }
}