using Infrastracture.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.Contract.Query;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin,ITAdmin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<Response<List<UserSummaryDto>>> GetUsers(
        [FromQuery] bool includeInactive = false,
        [FromQuery] UserRole? role = null,
        CancellationToken ct = default)
    {
        return await _mediator.Send(new GetUsersQuery
        {
            IncludeInactive = includeInactive,
            Role = role
        }, ct);
    }

    [HttpPost]
    public async Task<Response<UserSummaryDto>> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);

    [HttpPatch("{id:guid}/status")]
    public async Task<Response<UserSummaryDto>> SetUserActive(
        Guid id,
        [FromBody] SetUserActiveRequest request,
        CancellationToken ct)
    {
        return await _mediator.Send(new SetUserActiveCommand
        {
            UserId = id,
            IsActive = request.IsActive
        }, ct);
    }
}
