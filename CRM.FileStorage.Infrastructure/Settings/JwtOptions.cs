namespace CRM.FileStorage.Infrastructure.Settings;

public class JwtOptions
{
    public string Issuer { get; set; } = "CRM.Identity";
    public string Audience { get; set; } = "CRM.Identity.Clients";
    public int AccessTokenValidityInMinutes { get; set; } = 5;
    public int RefreshTokenValidityInMinutes { get; set; } = 30;
    public string PublicKey { get; set; } = string.Empty;
}