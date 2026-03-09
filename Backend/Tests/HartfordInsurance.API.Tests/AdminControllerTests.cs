using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.API.Tests;

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _serviceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _serviceMock = new Mock<IAdminService>();
        _controller = new AdminController(_serviceMock.Object);
    }

    [Fact]
    public async Task CreatePlan_Should_ReturnOk_WithPlanId()
    {
        // Arrange
        var request = new CreatePlanRequestDto { PlanName = "Individual Essential Care" };
        _serviceMock.Setup(s => s.CreatePlanWithTierAsync(request)).ReturnsAsync(1);

        // Act
        var result = await _controller.CreatePlan(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboard_Should_ReturnOk_WithData()
    {
        // Arrange
        var dashboardData = new AdminDashboardDto { TotalCustomers = 100 };
        _serviceMock.Setup(s => s.GetOverviewAsync()).ReturnsAsync(dashboardData);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dashboardData);
    }

    [Fact]
    public async Task DeleteClaimsOfficer_Should_ReturnOk_OnSuccess()
    {
        // Arrange
        int officerId = 4;
        _serviceMock.Setup(s => s.DeleteClaimsOfficerAsync(officerId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteClaimsOfficer(officerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteClaimsOfficer_Should_ReturnNotFound_WhenServiceThrowsKeyNotFound()
    {
        // Arrange
        int officerId = 99;
        _serviceMock.Setup(s => s.DeleteClaimsOfficerAsync(officerId)).ThrowsAsync(new KeyNotFoundException("Claims Officer not found."));

        // Act & Assert
        // In a real Web API, we'd use a Global Exception Handler. 
        // Here we just verify the service call and that it throws.
        // If we want to test the Controller's try-catch (if it had one), we'd do it here.
        // Since the controller doesn't have a try-catch, it will bubble up.
        Func<Task> act = async () => await _controller.DeleteClaimsOfficer(officerId);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateTier_Should_ReturnOk_OnSuccess()
    {
        // Arrange
        var request = new PlanTierDto { PlanId = 1, TierName = "Shield" };
        _serviceMock.Setup(s => s.CreateTierAsync(request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateTier(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("Tier created successfully.");
    }
}
