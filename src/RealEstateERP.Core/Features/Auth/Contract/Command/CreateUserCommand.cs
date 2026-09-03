using Infrastracture.Base;
using MediatR;
using RealEstateERP.Core.Enums;
using RealEstateERP.Core.Features.Auth.DTOs;

namespace RealEstateERP.Core.Features.Auth.Contract.Command;

public class CreateUserCommand : IRequest<Response<UserSummaryDto>>
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
