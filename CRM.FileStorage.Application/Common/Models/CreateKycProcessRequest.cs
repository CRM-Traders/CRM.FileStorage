namespace CRM.FileStorage.Application.Common.Models;

public class CreateKycProcessRequest
{
}

public class CreateKycProcessResponse
{
    public Guid KycProcessId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string ContinuationUrl { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
}