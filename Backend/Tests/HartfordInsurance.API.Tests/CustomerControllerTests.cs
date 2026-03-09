using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Xunit;

namespace HartfordInsurance.API.Tests;

public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _serviceMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _serviceMock = new Mock<ICustomerService>();
        _controller = new CustomerController(_serviceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new System.Security.Claims.Claim[] {
            new System.Security.Claims.Claim("userId", "5")
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
        _serviceMock.Setup(s => s.GetDashboardAsync(5)).ReturnsAsync(new CustomerDashboardDto());

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPlans_ReturnsOk()
    {
        // Act
        var result = await _controller.GetPlans();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RequestPolicy_CallsService()
    {
        // Arrange
        var request = new RequestPolicyDto { PlanId = 1, TierId = 1 };

        // Act
        var result = await _controller.RequestPolicy(request);

        // Assert
        _serviceMock.Verify(s => s.RequestPolicyAsync(request), Times.Once);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("Policy request submitted for approval.");
    }

    [Fact]
    public async Task RaiseClaim_CallsService()
    {
        // Arrange
        var claim = new ClaimDto { PolicyId = 1, ClaimAmount = 500 };

        // Act
        var result = await _controller.RaiseClaim(claim);

        // Assert
        _serviceMock.Verify(s => s.RaiseClaimAsync(claim), Times.Once);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task MakePayment_ReturnsOk()
    {
        // Arrange
        var request = new MakePaymentDto { PolicyId = 1 };
        _serviceMock.Setup(s => s.MakePaymentAsync(request)).ReturnsAsync("invoice_url");

        // Act
        var result = await _controller.MakePayment(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { invoiceUrl = "invoice_url", message = "Payment successful." });
    }

    [Fact]
    public async Task RenewPolicy_CallsService()
    {
        // Act
        var result = await _controller.RenewPolicy(1);

        // Assert
        _serviceMock.Verify(s => s.RenewPolicyAsync(1, 5), Times.Once);
        result.Should().BeOfType<OkObjectResult>();
    }
}
