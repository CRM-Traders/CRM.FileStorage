using CRM.FileStorage.Domain.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRM.FileStorage.Api.Controllers.Base;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IResult ToResult<TResponse>(Result<TResponse> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null)
            {
                return TypedResults.NoContent();
            }

            if (Request.Method == "POST" && (
                    typeof(TResponse) == typeof(Guid) ||
                    typeof(TResponse) == typeof(int) ||
                    typeof(TResponse) == typeof(string)))
            {
                string path = $"{Request.Path}/{result.Value}";
                var uri = new Uri(path, UriKind.Relative);
                return TypedResults.Created(uri, result.Value);
            }

            return TypedResults.Ok(result.Value);
        }

        if (result.ValidationErrors?.Count > 0)
        {
            return TypedResults.ValidationProblem(result.ValidationErrors);
        }

        if (!string.IsNullOrEmpty(result.ErrorCode))
        {
            int statusCode = result.ErrorCode switch
            {
                "NotFound" => StatusCodes.Status404NotFound,
                "Unauthorized" => StatusCodes.Status401Unauthorized,
                "Forbidden" => StatusCodes.Status403Forbidden,
                "Conflict" => StatusCodes.Status409Conflict,
                "PreconditionFailed" => StatusCodes.Status412PreconditionFailed,
                "TooManyRequests" => StatusCodes.Status429TooManyRequests,
                "PaymentRequired" => StatusCodes.Status402PaymentRequired,
                _ => StatusCodes.Status400BadRequest
            };

            return TypedResults.Problem(
                detail: result.Error,
                statusCode: statusCode,
                title: result.ErrorCode);
        }

        return TypedResults.Problem(
            detail: result.Error ?? "An unexpected error occurred",
            statusCode: StatusCodes.Status400BadRequest);
    }
}