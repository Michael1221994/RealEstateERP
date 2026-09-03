using Infrastracture.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateERP.Core.Features.Auth.Contract.Query;
using RealEstateERP.Core.Features.Auth.DTOs;
using RealEstateERP.Core.Models;

namespace RealEstateERP.Core.Features.Auth.Handler.Query;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Response<List<UserSummaryDto>>>
{
    private readonly IRepository _repo;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(IRepository repo, ILogger<GetUsersQueryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Response<List<UserSummaryDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        try
        {
            var query = await _repo.GetQueryAsync<User>();

            if (!request.IncludeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.Role == request.Role.Value);
            }

            var users = await query.OrderBy(u => u.Username).ToListAsync(ct);
            var result = users.Select(UserSummaryDto.From).ToList();

            return Response<List<UserSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list users");
            return SafeError.Unexpected<List<UserSummaryDto>>(ex);
        }
    }
}
