using System.Security.Cryptography;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.FileStorage.Infrastructure.Services;

public class FileStorageService(
    IOptions<FileStorageSettings> settings,
    ILogger<FileStorageService> logger)
    : IFileStorageService
{
    private readonly FileStorageSettings _settings = settings.Value;

    public async Task<string> SaveTemporaryFileAsync(Stream fileStream, string fileName, string? bucketName = null)
    {
        bucketName = string.IsNullOrEmpty(bucketName) ? "default" : bucketName;

        var directory = Path.Combine(_settings.BasePath, _settings.TempDirectory, bucketName);
        Directory.CreateDirectory(directory);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(directory, uniqueFileName);

        try
        {
            using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamWriter);

            logger.LogInformation("File {FileName} saved to temporary storage at {FilePath}", fileName, filePath);

            return uniqueFileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving file {FileName} to temporary storage", fileName);
            throw;
        }
    }

    public async Task<string> MoveToPermanentStorageAsync(string tempPath, string fileName, string tempBucket,
        string? permanentBucket = null)
    {
        permanentBucket = string.IsNullOrEmpty(permanentBucket) ? "default" : permanentBucket;

        var tempDirectory = Path.Combine(_settings.BasePath, _settings.TempDirectory, tempBucket);
        var permanentDirectory = Path.Combine(_settings.BasePath, _settings.PermanentDirectory, permanentBucket);

        Directory.CreateDirectory(permanentDirectory);

        var tempFilePath = Path.Combine(tempDirectory, tempPath);
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var permanentFilePath = Path.Combine(permanentDirectory, uniqueFileName);

        try
        {
            using var tempFileStream = new FileStream(tempFilePath, FileMode.Open);
            using var permanentFileStream = new FileStream(permanentFilePath, FileMode.Create);
            await tempFileStream.CopyToAsync(permanentFileStream);

            // We don't delete the temp file here - it will be cleaned up by the background service

            logger.LogInformation("File {FileName} moved from temporary to permanent storage at {FilePath}", fileName,
                permanentFilePath);

            return uniqueFileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error moving file {FileName} from temporary to permanent storage", fileName);
            throw;
        }
    }

    public Task DeleteFileAsync(string filePath, string bucketName)
    {
        try
        {
            var directory = Path.Combine(_settings.BasePath,
                bucketName.StartsWith("kyc-temp") ? _settings.TempDirectory : _settings.PermanentDirectory,
                bucketName);

            var fullPath = Path.Combine(directory, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation("File deleted from storage: {FilePath}", fullPath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file {FilePath} from bucket {BucketName}", filePath, bucketName);
            throw;
        }
    }

    public Task<Stream> ReadFileAsync(string filePath, string bucketName)
    {
        try
        {
            var directory = Path.Combine(_settings.BasePath,
                bucketName.StartsWith("kyc-temp") ? _settings.TempDirectory : _settings.PermanentDirectory,
                bucketName);

            var fullPath = Path.Combine(directory, filePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found at {fullPath}");
            }

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(fileStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading file {FilePath} from bucket {BucketName}", filePath, bucketName);
            throw;
        }
    }

    public async Task<string> CalculateFileHashAsync(Stream fileStream)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating file hash");
            throw;
        }
    }
}