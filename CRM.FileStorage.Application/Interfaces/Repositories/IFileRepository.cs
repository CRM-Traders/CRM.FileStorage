using CRM.FileStorage.Domain.Entities;

namespace CRM.FileStorage.Application.Interfaces.Repositories;

public interface IFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id);
    Task<IEnumerable<StoredFile>> GetFilesByUserIdAsync(Guid userId);
    Task<IEnumerable<StoredFile>> GetFilesByKycProcessIdAsync(Guid kycProcessId);
    Task<IEnumerable<StoredFile>> GetFilesByReferenceAsync(string reference);
    Task<IEnumerable<StoredFile>> GetExpiredTemporaryFilesAsync(DateTimeOffset olderThan);
    Task AddAsync(StoredFile file);
    Task UpdateAsync(StoredFile file);
    Task DeleteAsync(StoredFile file);
}