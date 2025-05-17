using CRM.FileStorage.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CRM.FileStorage.Application.Common.Models;

public class UploadKycFileRequest
{
    public string KycProcessIdOrToken { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UploadKycFileResponse
{
    public Guid FileId { get; set; }
    public Guid KycProcessId { get; set; }
    public FileType FileType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileStatus Status { get; set; }
}