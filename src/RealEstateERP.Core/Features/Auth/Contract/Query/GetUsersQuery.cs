using Infrastracture.Base;
using MediatR;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.Core.Features.Auth.Contract.Query;

public class GetUsersQuery : IRequest<Response<List<UserSummaryDto>>>
{
    /// <summary>When true, inactive users are included in the result.</summary>
    public bool IncludeInactive { get; set; }

    /// <summary>Optional role filter.</summary>
    public UserRole? Role { get; set; }
}
