using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Domain.Common.Models;
using CRM.FileStorage.Domain.Entities;
using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Services;

public class FileService(
    IFileRepository fileRepository,
    IFileStorageService fileStorageService,
    IFileValidationService fileValidationService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IFileService
{
  public async Task<Result<FileContentResponse>> GetFileContentAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);

        if (file == null || file.Status == FileStatus.Deleted)
            return Result.Failure<FileContentResponse>("ფაილი ვერ მოიძებნა", "NotFound");

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
            return Result.Failure<MakePermanentResponse>("ფაილი ვერ მოიძებნა", "NotFound");

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

        var permanentBucket = GetBucketName(file);

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
            return Result.Failure<bool>("ფაილი ვერ მოიძებნა", "NotFound");

        // შევამოწმოთ შეგვიძლია თუ არა ამ ფაილის წაშლა
        if (!currentUserContext.IsAdmin && file.UserId != currentUserContext.UserId)
            return Result.Failure<bool>("არ გაქვთ ამ ფაილის წაშლის უფლება", "Forbidden");

        await fileStorageService.DeleteFileAsync(file.StoragePath, file.BucketName);

        file.MarkAsDeleted();
        await fileRepository.UpdateAsync(file);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(true);
    }

    public async Task<Result<UploadFileResponse>> UploadFileAsync(UploadFileRequest request)
    { 
        if (currentUserContext.UserId == null)
            return Result.Failure<UploadFileResponse>("User is not authorized", "Unauthorized");
         
        if (!fileValidationService.IsValidFileSize(request.File.Length))
            return Result.Failure<UploadFileResponse>($"File Exceeds 10MB Limit", "BadRequest");
 
        if ((request.FileType == FileType.Image || request.FileType == FileType.FacePhoto || 
             request.FileType == FileType.IdFront || request.FileType == FileType.IdBack || 
             request.FileType == FileType.PassportMain) && 
            !fileValidationService.IsValidImageFile(request.File.FileName, request.File.ContentType))
        {
            return Result.Failure<UploadFileResponse>("Supported File Types: JPG, JPEG, PNG, GIF, BMP", "BadRequest");
        }
 
        var ownerId = request.OwnerId ?? currentUserContext.UserId.Value;
 
        var bucketName = $"user-{ownerId}";
        if (request.FileType == FileType.IdFront || request.FileType == FileType.IdBack || 
            request.FileType == FileType.PassportMain || request.FileType == FileType.FacePhoto)
        {
            bucketName = $"kyc-temp-{ownerId}";
        }
 
        await using var fileStream = request.File.OpenReadStream();
        var filePath = await fileStorageService.SaveTemporaryFileAsync(fileStream, request.File.FileName, bucketName);
 
        fileStream.Position = 0;
        var fileHash = await fileStorageService.CalculateFileHashAsync(fileStream);
 
        var file = new StoredFile(
            ownerId,
            request.File.FileName,
            fileValidationService.GetFileExtension(request.File.FileName),
            request.File.ContentType,
            request.File.Length,
            request.FileType,
            fileHash,
            filePath,
            bucketName);

        
        if (!string.IsNullOrEmpty(request.Reference))
        {
            file.SetReference(request.Reference);
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            file.SetDescription(request.Description);
        }

        
        await fileRepository.AddAsync(file);
        await unitOfWork.SaveChangesAsync();

        
        if (request.MakePermanent)
        {
            var permanentBucket = GetBucketName(file);
            var newPath = await fileStorageService.MoveToPermanentStorageAsync(
                file.StoragePath, 
                file.OriginalFileName,
                file.BucketName, 
                permanentBucket);
            
            file.MakePermanent(newPath, permanentBucket);
            await fileRepository.UpdateAsync(file);
            await unitOfWork.SaveChangesAsync();
        }

        return Result.Success(new UploadFileResponse
        {
            FileId = file.Id,
            FileType = file.FileType,
            FileName = file.OriginalFileName,
            FileSize = file.FileSize,
            Status = file.Status,
            FileUrl = $"/api/files/{file.Id}"
        });
    }

    public async Task<Result<IEnumerable<StoredFileDto>>> GetFilesByUserIdAsync(Guid userId, FileType? fileType = null)
    {
        
        if (!currentUserContext.IsAdmin && currentUserContext.UserId != userId)
            return Result.Failure<IEnumerable<StoredFileDto>>("არ გაქვთ სხვა მომხმარებლის ფაილების ნახვის უფლება", "Forbidden");

        var files = await fileRepository.GetFilesByUserIdAsync(userId);
        
        if (fileType.HasValue)
        {
            files = files.Where(f => f.FileType == fileType.Value);
        }

        var filesDtos = files.Where(f => f.Status != FileStatus.Deleted)
            .Select(MapToDto)
            .ToList();

        return Result.Success<IEnumerable<StoredFileDto>>(filesDtos);
    }

    public async Task<Result<IEnumerable<StoredFileDto>>> GetFilesByReferenceAsync(string reference)
    {
        if (string.IsNullOrEmpty(reference))
            return Result.Failure<IEnumerable<StoredFileDto>>("მითითება არ შეიძლება იყოს ცარიელი", "BadRequest");

        var files = await fileRepository.GetFilesByReferenceAsync(reference);
        
        
        if (!currentUserContext.IsAdmin)
        {
            files = files.Where(f => f.UserId == currentUserContext.UserId).ToList();
        }

        var filesDtos = files.Where(f => f.Status != FileStatus.Deleted)
            .Select(MapToDto)
            .ToList();

        return Result.Success<IEnumerable<StoredFileDto>>(filesDtos);
    }

    private string GetBucketName(StoredFile file)
    {
        if (file.FileType == FileType.IdFront || file.FileType == FileType.IdBack || 
            file.FileType == FileType.PassportMain || file.FileType == FileType.FacePhoto)
        {
            return $"kyc-{file.UserId}";
        }
        
        return $"user-{file.UserId}-permanent";
    }

    private StoredFileDto MapToDto(StoredFile file)
    {
        return new StoredFileDto
        {
            Id = file.Id,
            UserId = file.UserId,
            FileName = file.OriginalFileName,
            FileExtension = file.FileExtension,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FileType = file.FileType,
            Status = file.Status,
            BucketName = file.BucketName,
            KycProcessId = file.KycProcessId,
            CreationTime = file.CreationTime,
            FileUrl = $"/api/files/{file.Id}",
            Reference = file.Reference,
            Description = file.Description
        };
    }
}