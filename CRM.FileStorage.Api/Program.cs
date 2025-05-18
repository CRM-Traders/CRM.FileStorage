using System.Security.Cryptography;
using CRM.FileStorage.Api.Authentication;
using CRM.FileStorage.Api.Middleware;
using CRM.FileStorage.Api.Transformers;
using CRM.FileStorage.Application.DI;
using CRM.FileStorage.Infrastructure.DI;
using CRM.FileStorage.Infrastructure.Settings;
using CRM.FileStorage.Persistence.DI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);

builder.Services.AddOpenApi();


var app = builder.Build();
ConfigureMiddleware(app, app.Environment);

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("CRM FileStorage API")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Infrastructure
    services.AddHttpContextAccessor();
    services.AddApplication();
    services.AddInfrastructure(configuration);
    services.AddPersistence(configuration);
    // JWT  
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
    var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();

    if (jwtOptions?.PublicKey != null)
    {
        byte[] publicKeyBytes = Convert.FromBase64String(jwtOptions.PublicKey);
        RSA rsaPublicKey = RSA.Create();
        rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new RsaSecurityKey(rsaPublicKey),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddSingleton<JwtBearerHandler, TokenAuthenticationHandler>();
    }

    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });


    // Controllers & API
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    builder.Services.AddOpenApi(options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
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
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}


void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();


    app.UseHsts();

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
}