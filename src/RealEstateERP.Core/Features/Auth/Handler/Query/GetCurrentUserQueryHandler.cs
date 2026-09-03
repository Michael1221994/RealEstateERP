using Infrastracture.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateERP.Core.Features.Auth.Contract.Query;
using RealEstateERP.Core.Features.Auth.DTOs;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Core.Features.Auth.Handler.Query;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Response<UserSummaryDto>>
{
    private readonly IRepository _repo;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(IRepository repo, ILogger<GetCurrentUserQueryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Response<UserSummaryDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        try
        {
            var user = await (await _repo.GetQueryAsync<User>(u => u.Id == request.UserId)).FirstOrDefaultAsync(ct);

            if (user == null)
            {
                return Response<UserSummaryDto>.NotFound("User not found.");
            }

            return Response<UserSummaryDto>.Success(UserSummaryDto.From(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load current user {UserId}", request.UserId);
            return SafeError.Unexpected<UserSummaryDto>(ex);
        }
    }
}
