using Infrastracture.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateERP.API.Extensions;
using RealEstateERP.Core.Features.Auth.Contract.Command;
using RealEstateERP.Core.Features.Auth.Contract.Query;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<Response<LoginResponse>> Login([FromBody] LoginCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);

    [HttpGet("me")]
    public async Task<Response<UserSummaryDto>> Me(CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Response<UserSummaryDto>.Error("Unable to identify the current user from the token.");
        }

        return await _mediator.Send(new GetCurrentUserQuery { UserId = userId.Value }, ct);
    }
}
