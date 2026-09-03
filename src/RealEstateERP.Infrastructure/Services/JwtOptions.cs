namespace RealEstateERP.Infrastructure.Services;

public class JwtOptions
{
    /// <summary>Base64-encoded symmetric signing key (32+ bytes recommended).</summary>
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "RealEstateERP";

    public string Audience { get; set; } = "realestate-erp-api";

    public int ExpiryMinutes { get; set; } = 480;
}
