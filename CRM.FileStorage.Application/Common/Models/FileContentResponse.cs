namespace CRM.FileStorage.Application.Common.Models;

public class FileContentResponse
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Stream FileContent { get; set; } = Stream.Null;
}