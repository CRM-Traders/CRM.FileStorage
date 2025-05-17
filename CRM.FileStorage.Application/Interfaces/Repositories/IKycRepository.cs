using CRM.FileStorage.Domain.Entities;

namespace CRM.FileStorage.Application.Interfaces.Repositories;

public interface IKycRepository
{
    Task<KycProcess?> GetByIdAsync(Guid id);
    Task<KycProcess?> GetActiveProcessByUserIdAsync(Guid userId);
    Task<KycProcess?> GetBySessionTokenAsync(string sessionToken);
    Task AddAsync(KycProcess kycProcess);
    Task UpdateAsync(KycProcess kycProcess);
}