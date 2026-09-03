using Infrastracture.Base;
using MediatR;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.Core.Features.Auth.Contract.Query;

public class GetCurrentUserQuery : IRequest<Response<UserSummaryDto>>
{
    public Guid UserId { get; set; }
}
