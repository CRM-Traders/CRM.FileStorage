using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CRM.FileStorage.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public ClaimsPrincipal? ValidateToken(string token, out SecurityToken? validatedToken)
    {
        validatedToken = null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            byte[] publicKeyBytes = Convert.FromBase64String(_jwtOptions.PublicKey);

            using RSA rsaPublicKey = RSA.Create();
            rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new RsaSecurityKey(rsaPublicKey),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}