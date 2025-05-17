namespace CRM.FileStorage.Application.Common.Models;

public class MakePermanentRequest
{
    public Guid FileId { get; set; }
}

public class MakePermanentResponse
{
    public Guid FileId { get; set; }
    public string NewPath { get; set; } = string.Empty;
    public string NewBucket { get; set; } = string.Empty;
    public bool Success { get; set; }
}