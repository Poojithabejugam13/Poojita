using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Xunit;

namespace HartfordInsurance.API.Tests;

public class ClaimsOfficerControllerTests
{
    private readonly Mock<IClaimService> _serviceMock;
    private readonly ClaimsOfficerController _controller;

    public ClaimsOfficerControllerTests()
    {
        _serviceMock = new Mock<IClaimService>();
        _controller = new ClaimsOfficerController(_serviceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new System.Security.Claims.Claim[] {
            new System.Security.Claims.Claim("userId", "4")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetDashboardAsync()).ReturnsAsync(new ClaimsDashboardDto());

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPendingClaims_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetPendingClaimsAsync()).ReturnsAsync(new List<ClaimDto>());

        // Act
        var result = await _controller.GetPendingClaims();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ApproveClaim_CallsService()
    {
        // Act
        var result = await _controller.ApproveClaim(1, new ApproveRequestDto());

        // Assert
        _serviceMock.Verify(s => s.ApproveClaimAsync(1, It.IsAny<string?>()), Times.Once);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RejectClaim_CallsServiceWithReason()
    {
        // Arrange
        var dto = new RejectRequestDto { Reason = "Fraud" };

        // Act
        await _controller.RejectClaim(1, dto);

        // Assert
        _serviceMock.Verify(s => s.RejectClaimAsync(1, "Fraud"), Times.Once);
    }

    [Fact]
    public async Task GetMyCustomers_ReturnsData()
    {
        // Act
        var result = await _controller.GetMyCustomers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.GetMyCustomersAsync(4), Times.Once);
    }
}
