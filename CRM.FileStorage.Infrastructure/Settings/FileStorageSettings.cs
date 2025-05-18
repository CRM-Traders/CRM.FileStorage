namespace CRM.FileStorage.Infrastructure.Settings;

public class FileStorageSettings
{
    public string StorageType { get; set; } = "Local";
    
    public string TempBasePath { get; set; } = "/var/files/temp";
    public string PermanentBasePath { get; set; } = "/var/files/permanent";
    public string BasePath { get; set; } = "/var/files";
    public string TempDirectory { get; set; } = "temp";
    public string PermanentDirectory { get; set; } = "permanent";
     
    public int MaxFileSizeMb { get; set; } = 10;
    public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

    public string[] AllowedImageMimeTypes { get; set; } =
    {
        "image/jpeg",
        "image/pjpeg",
        "image/png",
        "image/gif",
        "image/bmp"
    };
     
    public string[] AllowedDocumentExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" };
    
    public string[] AllowedDocumentMimeTypes { get; set; } =
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain"
    };
}