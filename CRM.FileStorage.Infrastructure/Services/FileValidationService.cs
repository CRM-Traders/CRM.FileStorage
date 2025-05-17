using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace CRM.FileStorage.Infrastructure.Services;

public class FileValidationService(IOptions<FileStorageSettings> settings) : IFileValidationService
{
    private readonly FileStorageSettings _settings = settings.Value;

    public bool IsValidImageFile(string fileName, string contentType)
    {
        var extension = GetFileExtension(fileName).ToLowerInvariant();

        return _settings.AllowedImageExtensions.Contains(extension) &&
               _settings.AllowedImageMimeTypes.Contains(contentType);
    }

    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    public bool IsValidFileSize(long fileSize, int maxSizeInMb = 10)
    {
        var maxSize = maxSizeInMb == 0 ? _settings.MaxFileSizeMb : maxSizeInMb;
        return fileSize <= maxSize * 1024 * 1024;
    }
}