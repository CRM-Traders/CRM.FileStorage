using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Domain.Entities;
using CRM.FileStorage.Domain.Enums;
using CRM.FileStorage.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CRM.FileStorage.Persistence.Repositories;

public class FileRepository(FileStorageDbContext context) : IFileRepository
{
    public async Task<StoredFile?> GetByIdAsync(Guid id)
    {
        return await context.Files.FindAsync(id);
    }

    public async Task<IEnumerable<StoredFile>> GetFilesByUserIdAsync(Guid userId)
    {
        return await context.Files
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<StoredFile>> GetFilesByKycProcessIdAsync(Guid kycProcessId)
    {
        return await context.Files
            .Where(f => f.KycProcessId == kycProcessId)
            .ToListAsync();
    }

    public async Task<IEnumerable<StoredFile>> GetFilesByReferenceAsync(string reference)
    {
        return await context.Files
            .Where(f => f.Reference == reference)
            .ToListAsync();
    }

    public async Task<IEnumerable<StoredFile>> GetExpiredTemporaryFilesAsync(DateTimeOffset olderThan)
    {
        return await context.Files
            .Where(f => f.Status == FileStatus.Temporary &&
                        f.ExpirationTime.HasValue &&
                        f.ExpirationTime.Value < olderThan)
            .ToListAsync();
    }

    public async Task AddAsync(StoredFile file)
    {
        await context.Files.AddAsync(file);
    }

    public Task UpdateAsync(StoredFile file)
    {
        context.Files.Update(file);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(StoredFile file)
    {
        context.Files.Remove(file);
        return Task.CompletedTask;
    }
}