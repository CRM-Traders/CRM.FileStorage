using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace CRM.FileStorage.Api.Authentication;

public class TokenAuthenticationHandler(
    IOptionsMonitor<JwtBearerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : JwtBearerHandler(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = GetTokenFromAlternateSources();

        if (!string.IsNullOrEmpty(token))
        {
            Context.Request.Headers.Authorization = "Bearer " + token;
        }

        var result = await base.HandleAuthenticateAsync();

        return result;
    }

    private string GetTokenFromAlternateSources()
    {
        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authHeaderValue = authHeader.ToString();

            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(authHeaderValue))
            {
                return authHeaderValue.Trim();
            }
        }

        if (Request.Headers.TryGetValue("Token", out var tokenHeader))
        {
            return tokenHeader.ToString().Trim();
        }

        if (Request.Query.TryGetValue("token", out var queryToken))
        {
            return queryToken.ToString().Trim();
        }

        if (Request.Cookies.TryGetValue("token", out var cookieToken))
        {
            return cookieToken.Trim();
        }

        return null!;
    }
}