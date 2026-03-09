using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;

namespace HartfordInsurance.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository,
                       IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<string> LoginAsync(AuthDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || user.PasswordHash != request.Password)
            throw new UnauthorizedAccessException("Invalid email or password.");

        return _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString(), user.FullName);
    }

    public async Task RegisterCustomerAsync(AuthDto request)
    {
        await RegisterUserAsync(request, Role.Customer);
    }

    public async Task RegisterUserAsync(AuthDto request, Role role)
    {
        var exists = await _userRepository.GetByEmailAsync(request.Email);
        if (exists != null)
            throw new ArgumentException("Email already registered.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = request.Password,
            Role = role
        };

        await _userRepository.AddAsync(user);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || user.PhoneNumber != request.PhoneNumber)
            throw new ArgumentException("Verification mismatch: Email and Phone Number do not match our records.");

        user.PasswordHash = request.NewPassword;
        await _userRepository.UpdateAsync(user);
    }
}