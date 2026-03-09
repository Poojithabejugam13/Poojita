using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.API.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _serviceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _serviceMock = new Mock<IAuthService>();
        _controller = new AuthController(_serviceMock.Object);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsToken()
    {
        // Arrange
        var request = new AuthDto { Email = "test@test.com", Password = "password" };
        _serviceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync("jwt_token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { token = "jwt_token" });
    }

    [Fact]
    public async Task Login_InvalidCredentials_ThrowsException()
    {
        // Arrange
        var request = new AuthDto { Email = "fail@test.com" };
        _serviceMock.Setup(s => s.LoginAsync(request)).ThrowsAsync(new UnauthorizedAccessException("Invalid"));

        // Act & Assert
        Func<Task> act = async () => await _controller.Login(request);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new AuthDto { Email = "new@test.com" };

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("Customer registered successfully.");
        _serviceMock.Verify(s => s.RegisterCustomerAsync(request), Times.Once);
    }

    [Fact]
    public async Task Register_ExistingUser_ThrowsException()
    {
        // Arrange
        var request = new AuthDto { Email = "exists@test.com" };
        _serviceMock.Setup(s => s.RegisterCustomerAsync(request)).ThrowsAsync(new ArgumentException("Exists"));

        // Act & Assert
        Func<Task> act = async () => await _controller.Register(request);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void Constructor_SetsService()
    {
        // Assert
        _controller.Should().NotBeNull();
    }
}
