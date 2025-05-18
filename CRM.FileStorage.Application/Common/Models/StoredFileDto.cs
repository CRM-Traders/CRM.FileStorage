using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Common.Models;

public class StoredFileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileType FileType { get; set; }
    public FileStatus Status { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public Guid? KycProcessId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Description { get; set; }
}