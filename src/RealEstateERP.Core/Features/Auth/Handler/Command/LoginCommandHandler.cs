using Infrastracture.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.Contract.Service;
using RealEstateERP.Core.Features.Auth.DTOs;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Core.Features.Auth.Handler.Command;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Response<LoginResponse>>
{
    private readonly IRepository _repo;
    private readonly IAccessTokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IRepository repo,
        IAccessTokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _repo = repo;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Response<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var username = request.Username?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(request.Password))
        {
            return Response<LoginResponse>.Error("Username and password are required.");
        }

        try
        {
            var query = await _repo.GetQueryAsync<User>(u => u.Username == username, forUpdate: true);
            var user = await query.FirstOrDefaultAsync(ct);

            // Single generic message on purpose: do not reveal whether the username exists.
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for username {Username}", username);
                return Response<LoginResponse>.Error("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt on disabled account {UserId}", user.Id);
                return Response<LoginResponse>.Error("Your account is disabled. Contact an administrator.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);
            await _repo.UnitOfWork.SaveChanges();

            var accessToken = _tokenService.CreateToken(user);
            _logger.LogInformation("User {UserId} signed in", user.Id);

            return Response<LoginResponse>.Success(new LoginResponse
            {
                Token = accessToken.Token,
                ExpiresAtUtc = accessToken.ExpiresAtUtc,
                User = UserSummaryDto.From(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for username {Username}", username);
            return SafeError.Unexpected<LoginResponse>(ex);
        }
    }
}
