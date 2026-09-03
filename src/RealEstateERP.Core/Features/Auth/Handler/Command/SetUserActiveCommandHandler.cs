using Infrastracture.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.DTOs;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Core.Features.Auth.Handler.Command;

public class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Response<UserSummaryDto>>
{
    private readonly IRepository _repo;
    private readonly ILogger<SetUserActiveCommandHandler> _logger;

    public SetUserActiveCommandHandler(IRepository repo, ILogger<SetUserActiveCommandHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Response<UserSummaryDto>> Handle(SetUserActiveCommand request, CancellationToken ct)
    {
        try
        {
            var user = await (await _repo.GetQueryAsync<User>(u => u.Id == request.UserId, forUpdate: true))
                .FirstOrDefaultAsync(ct);

            if (user == null)
            {
                return Response<UserSummaryDto>.NotFound("User not found.");
            }

            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);
            await _repo.UnitOfWork.SaveChanges();

            _logger.LogInformation("User {UserId} active flag set to {IsActive}", user.Id, request.IsActive);
            return Response<UserSummaryDto>.Success(UserSummaryDto.From(user),
                request.IsActive ? "User activated." : "User deactivated.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change active flag for user {UserId}", request.UserId);
            return SafeError.Unexpected<UserSummaryDto>(ex);
        }
    }
}
