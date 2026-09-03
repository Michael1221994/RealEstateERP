using Infrastracture.Base;
using MediatR;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.Core.Features.Auth.Contract.Command;

public class LoginCommand : IRequest<Response<LoginResponse>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
