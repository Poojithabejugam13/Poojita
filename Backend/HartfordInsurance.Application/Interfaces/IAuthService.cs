using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Domain.Enums;

namespace HartfordInsurance.Application.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(AuthDto request);
    Task RegisterCustomerAsync(AuthDto request);
    Task RegisterUserAsync(AuthDto request, Role role);
    Task ResetPasswordAsync(ResetPasswordDto request);
}