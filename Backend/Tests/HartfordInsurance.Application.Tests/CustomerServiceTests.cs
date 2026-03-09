using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Application.Services;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Application.Tests;

public class CustomerServiceTests
{
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _planRepoMock = new Mock<IPlanRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _claimRepoMock = new Mock<IClaimRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _service = new CustomerService(_planRepoMock.Object, _policyRepoMock.Object, _claimRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_NoPolicy_AssignsAgentByRoundRobin()
    {
        // Arrange
        int customerId = 5;
        _policyRepoMock.Setup(r => r.GetPoliciesByCustomerAsync(customerId))
            .ReturnsAsync(new List<CustomerPolicy>());
        
        var agents = new List<User> { 
            new User { Id = 1, FullName = "Agent A" },
            new User { Id = 2, FullName = "Agent B" } 
        };
        _userRepoMock.Setup(r => r.GetUsersByRoleAsync(Role.Agent)).ReturnsAsync(agents);
        _userRepoMock.Setup(r => r.GetUsersByRoleAsync(Role.ClaimsOfficer)).ReturnsAsync(new List<User>());

        // Act
        var result = await _service.GetDashboardAsync(customerId);

        // Assert
        // 5 % 2 = 1 -> Agent B
        result.AssignedAgentName.Should().Be("Agent B");
    }

    [Fact]
    public async Task RequestPolicyAsync_TierMismatch_ThrowsException()
    {
        // Arrange
        var request = new RequestPolicyDto { PlanId = 1, TierId = 10 };
        _planRepoMock.Setup(r => r.GetTierByIdAsync(10)).ReturnsAsync(new PlanTier { PlanId = 2 });

        // Act
        Func<Task> act = async () => await _service.RequestPolicyAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Selected tier does not belong to this plan.");
    }

    [Fact]
    public async Task GetMyPoliciesAsync_ReturnsData()
    {
        // Arrange
        _policyRepoMock.Setup(r => r.GetPoliciesByCustomerAsync(1))
            .ReturnsAsync(new List<CustomerPolicy> { new CustomerPolicy { PolicyNumber = "POL1" } });

        // Act
        var result = await _service.GetMyPoliciesAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result[0].PolicyNumber.Should().Be("POL1");
    }

    [Fact]
    public async Task RenewPolicyAsync_UpdatesDates()
    {
        // Arrange
        var policy = new CustomerPolicy { PolicyId = 1, CustomerId = 5, ExpiryDate = new DateTime(2023, 1, 1), Status = PolicyStatus.Expired, CreatedAt = DateTime.UtcNow.AddYears(-1) };
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);
        _planRepoMock.Setup(r => r.GetTierByIdAsync(It.IsAny<int>())).ReturnsAsync(new PlanTier { BasePremium = 5000 });
        _claimRepoMock.Setup(r => r.GetClaimsByPolicyAsync(1)).ReturnsAsync(new List<Claim>());

        // Act
        await _service.RenewPolicyAsync(1, 5);

        // Assert
        policy.ExpiryDate.Year.Should().Be(DateTime.UtcNow.Year + 1);
        policy.Status.Should().Be(PolicyStatus.PendingApproval);
        _policyRepoMock.Verify(r => r.UpdateAsync(policy), Times.Once);
    }

    [Fact]
    public async Task RaiseClaimAsync_AddsClaim()
    {
        // Arrange
        var dto = new ClaimDto { PolicyId = 1, ClaimAmount = 5000, Reason = "Checkup" };

        // Act
        await _service.RaiseClaimAsync(dto);

        // Assert
        _claimRepoMock.Verify(r => r.AddAsync(It.Is<Claim>(c => c.ClaimAmount == 5000)), Times.Once);
    }
}
