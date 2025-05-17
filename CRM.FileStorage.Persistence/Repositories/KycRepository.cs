using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Domain.Entities;
using CRM.FileStorage.Domain.Enums;
using CRM.FileStorage.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CRM.FileStorage.Persistence.Repositories;

public class KycRepository(FileStorageDbContext context) : IKycRepository
{
    public async Task<KycProcess?> GetByIdAsync(Guid id)
    {
        return await context.KycProcesses
            .Include(k => k.Files)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<KycProcess?> GetActiveProcessByUserIdAsync(Guid userId)
    {
        return await context.KycProcesses
            .Include(k => k.Files)
            .Where(k => k.UserId == userId)
            .Where(k => k.Status != KycStatus.Verified && k.Status != KycStatus.Rejected)
            .OrderByDescending(k => k.LastActivityTime)
            .FirstOrDefaultAsync();
    }

    public async Task<KycProcess?> GetBySessionTokenAsync(string sessionToken)
    {
        return await context.KycProcesses
            .Include(k => k.Files)
            .FirstOrDefaultAsync(k => k.SessionToken == sessionToken);
    }

    public async Task AddAsync(KycProcess kycProcess)
    {
        await context.KycProcesses.AddAsync(kycProcess);
    }

    public Task UpdateAsync(KycProcess kycProcess)
    {
        context.KycProcesses.Update(kycProcess);
        return Task.CompletedTask;
    }
}