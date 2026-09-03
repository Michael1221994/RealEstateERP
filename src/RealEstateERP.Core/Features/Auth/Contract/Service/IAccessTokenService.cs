using RealEstateERP.Core.Models;

namespace RealEstateERP.Core.Features.Auth.Contract.Service;

public interface IAccessTokenService
{
    AccessToken CreateToken(User user);
}

public record AccessToken(string Token, DateTime ExpiresAtUtc);
