using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Domain.Common.Models;
using CRM.FileStorage.Domain.Enums;

namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IFileService
{
    Task<Result<FileContentResponse>> GetFileContentAsync(Guid fileId);
    Task<Result<MakePermanentResponse>> MakeFilePermanentAsync(MakePermanentRequest request);
    Task<Result<bool>> DeleteFileAsync(Guid fileId);
    Task<Result<UploadFileResponse>> UploadFileAsync(UploadFileRequest request);
    Task<Result<IEnumerable<StoredFileDto>>> GetFilesByUserIdAsync(Guid userId, FileType? fileType = null);
    Task<Result<IEnumerable<StoredFileDto>>> GetFilesByReferenceAsync(string reference);
}