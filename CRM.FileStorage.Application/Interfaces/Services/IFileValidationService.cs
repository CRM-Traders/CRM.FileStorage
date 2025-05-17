namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IFileValidationService
{
    bool IsValidImageFile(string fileName, string contentType);
    string GetFileExtension(string fileName);
    bool IsValidFileSize(long fileSize, int maxSizeInMb = 10);
}