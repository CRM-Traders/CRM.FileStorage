using System.Text.Json;
using CRM.FileStorage.Domain.Common.Models;
using FluentValidation;

namespace CRM.FileStorage.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var result = exception switch
        {
            ValidationException validationException => HandleValidationException(validationException),
            KeyNotFoundException => Result.Failure<object>("Resource not found", "NotFound"),
            UnauthorizedAccessException => Result.Failure<object>("Unauthorized access", "Unauthorized"),
            InvalidOperationException => Result.Failure<object>(exception.Message, "BadRequest"),
            _ => HandleUnknownException(exception)
        };

        context.Response.StatusCode = result.ErrorCode switch
        {
            "NotFound" => StatusCodes.Status404NotFound,
            "Unauthorized" => StatusCodes.Status401Unauthorized,
            "Forbidden" => StatusCodes.Status403Forbidden,
            "Conflict" => StatusCodes.Status409Conflict,
            "PreconditionFailed" => StatusCodes.Status412PreconditionFailed,
            "TooManyRequests" => StatusCodes.Status429TooManyRequests,
            "PaymentRequired" => StatusCodes.Status402PaymentRequired,
            _ => StatusCodes.Status500InternalServerError
        };

        var json = JsonSerializer.Serialize(result);
        await context.Response.WriteAsync(json);
    }

    private Result<object> HandleValidationException(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return Result.ValidationFailure<object>(errors);
    }

    private Result<object> HandleUnknownException(Exception exception)
    {
        var error = environment.IsDevelopment()
            ? $"{exception.Message} {exception.StackTrace}"
            : "An unexpected error occurred";

        return Result.Failure<object>(error, "InternalServerError");
    }
}