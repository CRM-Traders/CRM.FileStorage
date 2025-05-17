using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.FileStorage.Infrastructure.Services;

public class ExpiredFilesCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredFilesCleanupService> logger)
    : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Expired files cleanup service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Checking for expired files");

            try
            {
                await CleanupExpiredFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while cleaning up expired files");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredFilesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredFiles = await fileRepository.GetExpiredTemporaryFilesAsync(DateTimeOffset.UtcNow);

        foreach (var file in expiredFiles)
        {
            try
            {
                await fileStorageService.DeleteFileAsync(file.StoragePath, file.BucketName);

                file.MarkAsDeleted();
                await fileRepository.UpdateAsync(file);

                logger.LogInformation("Deleted expired file {FileId} from {BucketName}/{FilePath}",
                    file.Id, file.BucketName, file.StoragePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting expired file {FileId} from {BucketName}/{FilePath}",
                    file.Id, file.BucketName, file.StoragePath);
            }
        }

        if (expiredFiles.Any())
        {
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted {Count} expired files", expiredFiles.Count());
        }
    }
}