using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Xunit;

namespace HartfordInsurance.API.Tests;

public class AgentControllerTests
{
    private readonly Mock<IAgentService> _serviceMock;
    private readonly AgentController _controller;

    public AgentControllerTests()
    {
        _serviceMock = new Mock<IAgentService>();
        _controller = new AgentController(_serviceMock.Object);

        // Setup Mock ClaimsPrincipal (User)
        var user = new ClaimsPrincipal(new ClaimsIdentity(new System.Security.Claims.Claim[] {
            new System.Security.Claims.Claim("userId", "2"),
            new System.Security.Claims.Claim(ClaimTypes.Role, "Agent")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk_WithData()
    {
        // Arrange
        var dashboard = new AgentDashboardDto { PoliciesSold = 5 };
        _serviceMock.Setup(s => s.GetDashboardAsync(2)).ReturnsAsync(dashboard);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(dashboard);
    }

    [Fact]
    public async Task ApprovePolicy_ReturnsOk_OnSuccess()
    {
        // Act
        var result = await _controller.ApprovePolicy(1, new ApproveRequestDto());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.ApprovePolicyAsync(1, It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task RejectPolicy_ReturnsOk_OnSuccess()
    {
        // Arrange
        var dto = new RejectRequestDto { Reason = "Invalid" };

        // Act
        var result = await _controller.RejectPolicy(1, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.RejectPolicyAsync(1, "Invalid"), Times.Once);
    }

    [Fact]
    public async Task GetCommission_ReturnsOk_WithData()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetCommissionDetailsAsync(2)).ReturnsAsync(new AgentDashboardDto { TotalCommissionEarned = 1000 });

        // Act
        var result = await _controller.GetCommission();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ((AgentDashboardDto)okResult.Value!).TotalCommissionEarned.Should().Be(1000);
    }

    [Fact]
    public async Task GetMyCustomers_ReturnsOk_WithList()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetMyCustomersAsync(2)).ReturnsAsync(new List<CustomerListItemDto>());

        // Act
        var result = await _controller.GetMyCustomers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
