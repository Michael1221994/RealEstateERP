using Infrastracture.Base;
using MediatR;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.Core.Features.Auth.Contract.Command;

public class SetUserActiveCommand : IRequest<Response<UserSummaryDto>>
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}
