using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CRM.FileStorage.Application.Interfaces.Services;

public interface IJwtTokenService
{
    ClaimsPrincipal? ValidateToken(string token, out SecurityToken? validatedToken);
}