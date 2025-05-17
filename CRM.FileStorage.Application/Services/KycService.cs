using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Domain.Common.Models;
using CRM.FileStorage.Domain.Entities;
using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Services;

public class KycService(
    IKycRepository kycRepository,
    IFileRepository fileRepository,
    IFileStorageService fileStorageService,
    IFileValidationService fileValidationService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IKycService
{
    public async Task<Result<CreateKycProcessResponse>> CreateKycProcessAsync(CreateKycProcessRequest request)
    {
        if (currentUserContext.UserId == null)
            return Result.Failure<CreateKycProcessResponse>("User must be authenticated", "Unauthorized");

        var existingProcess = await kycRepository.GetActiveProcessByUserIdAsync(currentUserContext.UserId.Value);

        if (existingProcess != null)
        {
            existingProcess.UpdateActivity();
            await unitOfWork.SaveChangesAsync();

            return Result.Success(new CreateKycProcessResponse
            {
                KycProcessId = existingProcess.Id,
                SessionToken = existingProcess.SessionToken,
                ContinuationUrl = $"/kyc/process/{existingProcess.SessionToken}",
                QrCodeUrl = $"/kyc/qr/{existingProcess.SessionToken}"
            });
        }

        var kycProcess = new KycProcess(currentUserContext.UserId.Value);

        await kycRepository.AddAsync(kycProcess);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CreateKycProcessResponse
        {
            KycProcessId = kycProcess.Id,
            SessionToken = kycProcess.SessionToken,
            ContinuationUrl = $"/kyc/process/{kycProcess.SessionToken}",
            QrCodeUrl = $"/kyc/qr/{kycProcess.SessionToken}"
        });
    }

    public async Task<Result<KycProcessDto>> GetKycProcessAsync(string kycProcessIdOrToken)
    {
        KycProcess? kycProcess;

        if (Guid.TryParse(kycProcessIdOrToken, out var kycProcessId))
            kycProcess = await kycRepository.GetByIdAsync(kycProcessId);
        else
            kycProcess = await kycRepository.GetBySessionTokenAsync(kycProcessIdOrToken);

        if (kycProcess == null)
            return Result.Failure<KycProcessDto>("KYC process not found", "NotFound");

        var files = await fileRepository.GetFilesByKycProcessIdAsync(kycProcess.Id);

        var kycProcessDto = new KycProcessDto
        {
            Id = kycProcess.Id,
            UserId = kycProcess.UserId,
            Status = kycProcess.Status,
            SessionToken = kycProcess.SessionToken,
            CreatedAt = kycProcess.CreatedAt,
            LastActivityTime = kycProcess.LastActivityTime,
            VerificationComment = kycProcess.VerificationComment,
            ReviewedBy = kycProcess.ReviewedBy,
            ReviewedAt = kycProcess.ReviewedAt,
            Files = files.Where(f => f.Status != FileStatus.Deleted)
                .Select(f => new StoredFileDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    FileName = f.OriginalFileName,
                    FileExtension = f.FileExtension,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    FileType = f.FileType,
                    Status = f.Status,
                    BucketName = f.BucketName,
                    KycProcessId = f.KycProcessId,
                    CreationTime = f.CreationTime,
                    FileUrl = $"/api/files/{f.Id}"
                })
        };

        return Result.Success(kycProcessDto);
    }

    public async Task<Result<UploadKycFileResponse>> UploadKycFileAsync(UploadKycFileRequest request)
    {
        // Validate file
        if (!fileValidationService.IsValidImageFile(request.File.FileName, request.File.ContentType))
            return Result.Failure<UploadKycFileResponse>("Only image files are allowed", "BadRequest");

        if (!fileValidationService.IsValidFileSize(request.File.Length))
            return Result.Failure<UploadKycFileResponse>("File size must not exceed 10MB", "BadRequest");

        // Get KYC process
        KycProcess? kycProcess;

        if (Guid.TryParse(request.KycProcessIdOrToken, out var kycProcessId))
            kycProcess = await kycRepository.GetByIdAsync(kycProcessId);
        else
            kycProcess = await kycRepository.GetBySessionTokenAsync(request.KycProcessIdOrToken);

        if (kycProcess == null)
            return Result.Failure<UploadKycFileResponse>("KYC process not found", "NotFound");

        // Check authorization
        if (currentUserContext.UserId.HasValue && currentUserContext.UserId.Value != kycProcess.UserId)
            return Result.Failure<UploadKycFileResponse>("You don't have permission to access this KYC process",
                "Forbidden");

        // Save file to temporary storage
        await using var fileStream = request.File.OpenReadStream();

        var bucketName = $"kyc-temp-{kycProcess.UserId}";

        var filePath = await fileStorageService.SaveTemporaryFileAsync(fileStream, request.File.FileName, bucketName);

        // Calculate file hash
        fileStream.Position = 0;
        var fileHash = await fileStorageService.CalculateFileHashAsync(fileStream);

        // Create file entity
        var file = new StoredFile(
            kycProcess.UserId,
            request.File.FileName,
            fileValidationService.GetFileExtension(request.File.FileName),
            request.File.ContentType,
            request.File.Length,
            request.FileType,
            fileHash,
            filePath,
            bucketName,
            kycProcess.Id);

        // Add file to KYC process
        try
        {
            kycProcess.AddFile(file);
            await fileRepository.AddAsync(file);

            await unitOfWork.SaveChangesAsync();

            return Result.Success(new UploadKycFileResponse
            {
                FileId = file.Id,
                KycProcessId = kycProcess.Id,
                FileType = file.FileType,
                FileName = file.OriginalFileName,
                FileSize = file.FileSize,
                Status = file.Status
            });
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<UploadKycFileResponse>(ex.Message, "BadRequest");
        }
    }

    public async Task<Result<QrCodeResponseModel>> GetKycQrCodeAsync(string kycProcessIdOrToken, string baseUrl)
    {
        KycProcess? kycProcess;

        if (Guid.TryParse(kycProcessIdOrToken, out var kycProcessId))
            kycProcess = await kycRepository.GetByIdAsync(kycProcessId);
        else
            kycProcess = await kycRepository.GetBySessionTokenAsync(kycProcessIdOrToken);

        if (kycProcess == null)
            return Result.Failure<QrCodeResponseModel>("KYC process not found", "NotFound");

        var continueUrl = $"{baseUrl.TrimEnd('/')}/kyc/process/{kycProcess.SessionToken}";


        return Result.Success(new QrCodeResponseModel
        {
            KycProcessId = kycProcess.Id,
            SessionToken = kycProcess.SessionToken,
            ContinuationUrl = continueUrl
        });
    }

    public async Task<Result<VerifyKycProcessResponse>> VerifyKycProcessAsync(VerifyKycProcessRequest request)
    {
        if (!currentUserContext.IsAdmin)
            return Result.Failure<VerifyKycProcessResponse>("Only administrators can verify KYC processes",
                "Forbidden");

        var kycProcess = await kycRepository.GetByIdAsync(request.KycProcessId);

        if (kycProcess == null)
            return Result.Failure<VerifyKycProcessResponse>("KYC process not found", "NotFound");

        if (kycProcess.Status != KycStatus.DocumentsUploaded && kycProcess.Status != KycStatus.UnderReview)
            return Result.Failure<VerifyKycProcessResponse>(
                $"KYC process is not ready for verification, current status: {kycProcess.Status}", "BadRequest");

        try
        {
            kycProcess.CompleteVerification(request.IsApproved, request.Comment, currentUserContext.UserId!.Value);

            await kycRepository.UpdateAsync(kycProcess);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(new VerifyKycProcessResponse
            {
                KycProcessId = kycProcess.Id,
                Status = kycProcess.Status,
                Comment = kycProcess.VerificationComment
            });
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<VerifyKycProcessResponse>(ex.Message, "BadRequest");
        }
    }
}