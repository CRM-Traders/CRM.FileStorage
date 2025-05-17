namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IQrCodeService
{
    Task<byte[]> GenerateQrCodeAsync(string content, int size = 300);
    Task<string> GenerateQrCodeAsBase64Async(string content, int size = 300);
}