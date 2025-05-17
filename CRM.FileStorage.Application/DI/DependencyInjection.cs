using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.FileStorage.Application.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IKycService, KycService>();
        services.AddScoped<IFileService, FileService>();
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        return services;
    }
}