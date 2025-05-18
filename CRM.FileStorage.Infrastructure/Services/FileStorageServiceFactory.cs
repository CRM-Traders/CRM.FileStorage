using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.FileStorage.Infrastructure.Services;

public class FileStorageServiceFactory : IFileStorageService
{
    private readonly IFileStorageService _implementation;

    public FileStorageServiceFactory(
        IOptions<FileStorageSettings> fileStorageSettings,
        IOptions<MinioSettings> minioSettings,
        ILogger<FileStorageService> localLogger,
        ILogger<MinioStorageService> minioLogger)
    {
        var settings = fileStorageSettings.Value;

        _implementation = settings.StorageType.ToLowerInvariant() switch
        {
            "minio" => new MinioStorageService(minioSettings, minioLogger),
            _ => new FileStorageService(fileStorageSettings, localLogger)
        };
    }

    public Task<string> CalculateFileHashAsync(Stream fileStream) =>
        _implementation.CalculateFileHashAsync(fileStream);

    public Task DeleteFileAsync(string filePath, string bucketName) =>
        _implementation.DeleteFileAsync(filePath, bucketName);

    public Task<string> MoveToPermanentStorageAsync(string tempPath, string fileName, string tempBucket,
        string? permanentBucket = null) =>
        _implementation.MoveToPermanentStorageAsync(tempPath, fileName, tempBucket, permanentBucket);

    public Task<Stream> ReadFileAsync(string filePath, string bucketName) =>
        _implementation.ReadFileAsync(filePath, bucketName);

    public Task<string> SaveTemporaryFileAsync(Stream fileStream, string fileName, string? bucketName = null) =>
        _implementation.SaveTemporaryFileAsync(fileStream, fileName, bucketName);
}