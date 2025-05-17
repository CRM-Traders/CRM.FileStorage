namespace CRM.FileStorage.Infrastructure.Settings;

public class FileStorageSettings
{
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
}