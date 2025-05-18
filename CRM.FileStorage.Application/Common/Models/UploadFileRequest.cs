using CRM.FileStorage.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CRM.FileStorage.Application.Common.Models;

public class UploadFileRequest
{
    public FileType FileType { get; set; } = FileType.Other;
    public IFormFile File { get; set; } = null!;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public Guid? OwnerId { get; set; }
    public bool MakePermanent { get; set; } = false;
}

public class UploadFileResponse
{
    public Guid FileId { get; set; }
    public FileType FileType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileStatus Status { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}