using CRM.FileStorage.Api.Middleware;
using CRM.FileStorage.Application.DI;
using CRM.FileStorage.Infrastructure.DI;
using CRM.FileStorage.Persistence.DI;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddOpenApi();


var app = builder.Build();
ConfigureMiddleware(app, app.Environment);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("CRM FileStorage API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Infrastructure
    services.AddHttpContextAccessor();
    services.AddApplication();
    services.AddInfrastructure(configuration);
    services.AddPersistence(configuration);

    // Controllers & API
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // CORS policy
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}

void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
{
    // Exception handling
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Swagger for development
    if (env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();
}