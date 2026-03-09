using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Application.Services;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Application.Tests;

public class AdminServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly AdminService _service;

    public AdminServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _planRepoMock = new Mock<IPlanRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _claimRepoMock = new Mock<IClaimRepository>();
        _service = new AdminService(_userRepoMock.Object, _planRepoMock.Object, _policyRepoMock.Object, _claimRepoMock.Object);
    }

    [Fact]
    public async Task CreatePlanWithTierAsync_Should_ReturnPlanId_OnSuccess()
    {
        // Arrange
        var request = new CreatePlanRequestDto
        {
            PlanName = "Family FloaterPro",
            PlanType = "Family",
            TierName = "Elevate",
            BasePremium = 18499
        };
        _planRepoMock.Setup(r => r.AddPlanAsync(It.IsAny<InsurancePlan>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreatePlanWithTierAsync(request);

        // Assert
        result.Should().Be(1);
        _planRepoMock.Verify(r => r.AddPlanAsync(It.Is<InsurancePlan>(p => p.PlanName == "Family FloaterPro")), Times.Once);
        _planRepoMock.Verify(r => r.AddTierAsync(It.Is<PlanTier>(t => t.TierName == "Elevate" && t.BasePremium == 18499)), Times.Once);
    }

    [Fact]
    public async Task DeleteClaimsOfficerAsync_Should_ThrowKeyNotFoundException_WhenOfficerDoesNotExist()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _service.DeleteClaimsOfficerAsync(99);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Claims Officer not found.");
    }

    [Fact]
    public async Task DeleteClaimsOfficerAsync_Should_ThrowArgumentException_WhenOfficerHasAssignedPolicies()
    {
        // Arrange
        int officerId = 4;
        _userRepoMock.Setup(r => r.GetByIdAsync(officerId)).ReturnsAsync(new User { Id = officerId, Role = Role.ClaimsOfficer });
        _policyRepoMock.Setup(r => r.GetPoliciesByClaimsOfficerAsync(officerId))
            .ReturnsAsync(new List<CustomerPolicy> { new CustomerPolicy { PolicyId = 1 } });

        // Act
        Func<Task> act = async () => await _service.DeleteClaimsOfficerAsync(officerId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Cannot delete this Claims Officer because they are assigned to existing customer policies.");
    }

    [Fact]
    public async Task GetDashboardAsync_Should_ReturnCorrectStatistics()
    {
        // Arrange
        _userRepoMock.Setup(r => r.CountByRoleAsync(Role.Agent)).ReturnsAsync(2);
        _userRepoMock.Setup(r => r.CountByRoleAsync(Role.Customer)).ReturnsAsync(10);
        _policyRepoMock.Setup(r => r.CountAllAsync()).ReturnsAsync(15);

        // Act
        var result = await _service.GetDashboardAsync();

        // Assert
        result.TotalAgents.Should().Be(2);
        result.TotalCustomers.Should().Be(10);
        result.TotalPolicies.Should().Be(15);
    }
}
