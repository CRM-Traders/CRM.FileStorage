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
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        services.Configure<MinioSettings>(configuration.GetSection(nameof(MinioSettings)));

        services.AddSingleton<ICurrentUserContext, CurrentUserContext>();
        services.AddSingleton<IFileValidationService, FileValidationService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IFileStorageService, FileStorageServiceFactory>();

        services.AddHostedService<ExpiredFilesCleanupService>();

        return services;
    }
}