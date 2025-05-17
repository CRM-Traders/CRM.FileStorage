using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Domain.Common.Models;
using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Services;

public class FileService(
    IFileRepository fileRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork)
    : IFileService
{
    public async Task<Result<FileContentResponse>> GetFileContentAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);

        if (file == null || file.Status == FileStatus.Deleted)
            return Result.Failure<FileContentResponse>("File not found", "NotFound");

        var fileContent = await fileStorageService.ReadFileAsync(file.StoragePath, file.BucketName);

        return Result.Success(new FileContentResponse
        {
            FileId = file.Id,
            FileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FileContent = fileContent
        });
    }

    public async Task<Result<MakePermanentResponse>> MakeFilePermanentAsync(MakePermanentRequest request)
    {
        var file = await fileRepository.GetByIdAsync(request.FileId);

        if (file == null || file.Status == FileStatus.Deleted)
            return Result.Failure<MakePermanentResponse>("File not found", "NotFound");

        if (file.Status == FileStatus.Permanent)
        {
            return Result.Success(new MakePermanentResponse
            {
                FileId = file.Id,
                NewPath = file.StoragePath,
                NewBucket = file.BucketName,
                Success = true
            });
        }

        var permanentBucket = $"kyc-{file.UserId}";

        var newPath = await fileStorageService.MoveToPermanentStorageAsync(
            file.StoragePath,
            file.OriginalFileName,
            file.BucketName,
            permanentBucket);

        file.MakePermanent(newPath, permanentBucket);
        await fileRepository.UpdateAsync(file);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new MakePermanentResponse
        {
            FileId = file.Id,
            NewPath = newPath,
            NewBucket = permanentBucket,
            Success = true
        });
    }

    public async Task<Result<bool>> DeleteFileAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);

        if (file == null || file.Status == FileStatus.Deleted)
            return Result.Failure<bool>("File not found", "NotFound");

        await fileStorageService.DeleteFileAsync(file.StoragePath, file.BucketName);

        file.MarkAsDeleted();
        await fileRepository.UpdateAsync(file);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(true);
    }
}