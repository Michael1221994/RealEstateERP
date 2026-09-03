using Infrastracture.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.DTOs;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Core.Features.Auth.Handler.Command;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserSummaryDto>>
{
    private readonly IRepository _repo;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IRepository repo, ILogger<CreateUserCommandHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Response<UserSummaryDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var username = request.Username?.Trim().ToLowerInvariant() ?? string.Empty;
        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(username) || username.Length < 3)
        {
            return Response<UserSummaryDto>.Error("Username is required and must be at least 3 characters.");
        }

        if (string.IsNullOrEmpty(fullName))
        {
            return Response<UserSummaryDto>.Error("Full name is required.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
        {
            return Response<UserSummaryDto>.Error("Password is required and must be at least 6 characters.");
        }

        try
        {
            var existing = await (await _repo.GetQueryAsync<User>(u => u.Username == username)).AnyAsync(ct);
            if (existing)
            {
                return Response<UserSummaryDto>.Error($"Username '{username}' is already taken.");
            }

            var user = new User
            {
                FullName = fullName,
                Username = username,
                Email = request.Email?.Trim(),
                Phone = request.Phone?.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(user);
            await _repo.UnitOfWork.SaveChanges();

            _logger.LogInformation("Created user {UserId} with role {Role}", user.Id, user.Role);
            return Response<UserSummaryDto>.Success(UserSummaryDto.From(user), "User created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Username}", username);
            return SafeError.Unexpected<UserSummaryDto>(ex);
        }
    }
}
