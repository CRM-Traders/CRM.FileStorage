using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Domain.Common.Models;

namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IFileService
{
    Task<Result<FileContentResponse>> GetFileContentAsync(Guid fileId);
    Task<Result<MakePermanentResponse>> MakeFilePermanentAsync(MakePermanentRequest request);
    Task<Result<bool>> DeleteFileAsync(Guid fileId);
}