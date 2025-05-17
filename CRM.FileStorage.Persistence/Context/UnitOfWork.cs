using CRM.FileStorage.Application.Interfaces;

namespace CRM.FileStorage.Persistence.Context;

public class UnitOfWork(FileStorageDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}