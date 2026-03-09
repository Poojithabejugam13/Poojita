using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Application.Services;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _service = new AuthService(_userRepoMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new AuthDto { Email = "test@test.com", Password = "password" };
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "password", Role = Role.Customer };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(s => s.GenerateToken(user.Id, user.Email, "Customer", user.FullName)).Returns("valid_token");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().Be("valid_token");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new AuthDto { Email = "test@test.com", Password = "wrong_password" };
        var user = new User { Email = "test@test.com", PasswordHash = "password" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _service.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task RegisterUserAsync_NewEmail_AddsUser()
    {
        // Arrange
        var request = new AuthDto { Email = "new@test.com", Password = "password", FullName = "New User" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        await _service.RegisterUserAsync(request, Role.Customer);

        // Assert
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "new@test.com" && u.Role == Role.Customer)), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ExistingEmail_ThrowsArgumentException()
    {
        // Arrange
        var request = new AuthDto { Email = "existing@test.com" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(new User());

        // Act
        Func<Task> act = async () => await _service.RegisterUserAsync(request, Role.Agent);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Email already registered.");
    }

    [Fact]
    public async Task RegisterCustomerAsync_CallsRegisterWithCustomerRole()
    {
        // Arrange
        var request = new AuthDto { Email = "cust@test.com" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        await _service.RegisterCustomerAsync(request);

        // Assert
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Role == Role.Customer)), Times.Once);
    }
}
