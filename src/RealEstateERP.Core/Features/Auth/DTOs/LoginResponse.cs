namespace RealEstateERP.Core.Features.Auth.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserSummaryDto User { get; set; } = new();
}
