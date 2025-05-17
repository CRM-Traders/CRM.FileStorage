using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Application.Interfaces.Repositories;
using CRM.FileStorage.Persistence.Context;
using CRM.FileStorage.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.FileStorage.Persistence.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FileStorageDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FileStorageConnection"),
                b => b.MigrationsAssembly(typeof(FileStorageDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IKycRepository, KycRepository>();

        return services;
    }
}