namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveTemporaryFileAsync(Stream fileStream, string fileName, string? bucketName = null);
    Task<string> MoveToPermanentStorageAsync(string tempPath, string fileName, string tempBucket, string? permanentBucket = null);
    Task DeleteFileAsync(string filePath, string bucketName);
    Task<Stream> ReadFileAsync(string filePath, string bucketName);
    Task<string> CalculateFileHashAsync(Stream fileStream);
}