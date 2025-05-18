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

        string directory;
        if (!string.IsNullOrEmpty(_settings.TempBasePath))
        {
            directory = Path.Combine(_settings.TempBasePath, bucketName);
        }
        else
        {
            directory = Path.Combine(_settings.BasePath, _settings.TempDirectory, bucketName);
        }

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

        string tempDirectory;
        if (!string.IsNullOrEmpty(_settings.TempBasePath))
        {
            tempDirectory = Path.Combine(_settings.TempBasePath, tempBucket);
        }
        else
        {
            tempDirectory = Path.Combine(_settings.BasePath, _settings.TempDirectory, tempBucket);
        }

        string permanentDirectory;
        if (!string.IsNullOrEmpty(_settings.PermanentBasePath))
        {
            permanentDirectory = Path.Combine(_settings.PermanentBasePath, permanentBucket);
        }
        else
        {
            permanentDirectory = Path.Combine(_settings.BasePath, _settings.PermanentDirectory, permanentBucket);
        }

        Directory.CreateDirectory(permanentDirectory);

        var tempFilePath = Path.Combine(tempDirectory, tempPath);
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var permanentFilePath = Path.Combine(permanentDirectory, uniqueFileName);

        try
        {
            await using var tempFileStream = new FileStream(tempFilePath, FileMode.Open);
            await using var permanentFileStream = new FileStream(permanentFilePath, FileMode.Create);
            await tempFileStream.CopyToAsync(permanentFileStream);

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
            string directory;


            bool isTemp = bucketName.StartsWith("kyc-temp") || bucketName.Contains("temp");

            if (isTemp)
            {
                if (!string.IsNullOrEmpty(_settings.TempBasePath))
                {
                    directory = Path.Combine(_settings.TempBasePath, bucketName);
                }
                else
                {
                    directory = Path.Combine(_settings.BasePath, _settings.TempDirectory, bucketName);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_settings.PermanentBasePath))
                {
                    directory = Path.Combine(_settings.PermanentBasePath, bucketName);
                }
                else
                {
                    directory = Path.Combine(_settings.BasePath, _settings.PermanentDirectory, bucketName);
                }
            }

            var fullPath = Path.Combine(directory, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation("File deleted: {FilePath}", fullPath);
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
            string directory;
 
            bool isTemp = bucketName.StartsWith("kyc-temp") || bucketName.Contains("temp");

            if (isTemp)
            {
                if (!string.IsNullOrEmpty(_settings.TempBasePath))
                {
                    directory = Path.Combine(_settings.TempBasePath, bucketName);
                }
                else
                {
                    directory = Path.Combine(_settings.BasePath, _settings.TempDirectory, bucketName);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_settings.PermanentBasePath))
                {
                    directory = Path.Combine(_settings.PermanentBasePath, bucketName);
                }
                else
                {
                    directory = Path.Combine(_settings.BasePath, _settings.PermanentDirectory, bucketName);
                }
            }

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