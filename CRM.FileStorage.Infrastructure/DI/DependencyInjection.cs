using CRM.FileStorage.Application.Interfaces;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Infrastructure.Services;
using CRM.FileStorage.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.FileStorage.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageSettings>(configuration.GetSection(nameof(FileStorageSettings)));
        services.Configure<QrCodeSettings>(configuration.GetSection(nameof(QrCodeSettings)));
        
        services.AddSingleton<ICurrentUserContext, CurrentUserContext>();
        services.AddSingleton<IFileValidationService, FileValidationService>(); 
        
        services.AddScoped<IFileStorageService, FileStorageService>();
        
        services.AddHostedService<ExpiredFilesCleanupService>();
        
        return services;
    }
}