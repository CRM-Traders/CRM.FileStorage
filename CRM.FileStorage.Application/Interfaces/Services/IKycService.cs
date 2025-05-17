using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Domain.Common.Models;

namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IKycService
{
    Task<Result<CreateKycProcessResponse>> CreateKycProcessAsync(CreateKycProcessRequest request);
    Task<Result<KycProcessDto>> GetKycProcessAsync(string kycProcessIdOrToken);
    Task<Result<UploadKycFileResponse>> UploadKycFileAsync(UploadKycFileRequest request);
    Task<Result<QrCodeResponseModel>> GetKycQrCodeAsync(string kycProcessIdOrToken, string baseUrl);
    Task<Result<VerifyKycProcessResponse>> VerifyKycProcessAsync(VerifyKycProcessRequest request);
}