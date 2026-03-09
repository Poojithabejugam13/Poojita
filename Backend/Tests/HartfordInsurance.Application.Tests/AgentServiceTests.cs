using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Application.Services;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Application.Tests;

public class AgentServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly AgentService _service;

    public AgentServiceTests()
    {
        _policyRepoMock = new Mock<IPolicyRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _planRepoMock = new Mock<IPlanRepository>();
        _service = new AgentService(_policyRepoMock.Object, _userRepoMock.Object, _planRepoMock.Object);
    }

    [Fact]
    public async Task ApprovePolicyAsync_ValidPolicy_UpdatesStatusToApproved()
    {
        // Arrange
        var policy = new CustomerPolicy { PolicyId = 1, Status = PolicyStatus.PendingApproval };
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);

        // Act
        await _service.ApprovePolicyAsync(1);

        // Assert
        policy.Status.Should().Be(PolicyStatus.Approved);
        _policyRepoMock.Verify(r => r.UpdateAsync(policy), Times.Once);
    }

    [Fact]
    public async Task ApprovePolicyAsync_NotInPendingState_ThrowsException()
    {
        // Arrange
        var policy = new CustomerPolicy { PolicyId = 1, Status = PolicyStatus.Active };
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);

        // Act
        Func<Task> act = async () => await _service.ApprovePolicyAsync(1);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Policy is not in PendingApproval state.");
    }

    [Fact]
    public async Task RejectPolicyAsync_SetsStatusAndReason()
    {
        // Arrange
        var policy = new CustomerPolicy { PolicyId = 1 };
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);

        // Act
        await _service.RejectPolicyAsync(1, "Incomplete documentation");

        // Assert
        policy.Status.Should().Be(PolicyStatus.Rejected);
        policy.RejectionReason.Should().Be("Incomplete documentation");
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsAggregatedData()
    {
        // Arrange
        int agentId = 2;
        _policyRepoMock.Setup(r => r.CountPoliciesByAgentAsync(agentId)).ReturnsAsync(5);
        _policyRepoMock.Setup(r => r.CalculateCommissionAsync(agentId)).ReturnsAsync(2500.50m);

        // Act
        var result = await _service.GetDashboardAsync(agentId);

        // Assert
        result.PoliciesSold.Should().Be(5);
        result.TotalCommissionEarned.Should().Be(2500.50m);
    }

    [Fact]
    public async Task GetMyCustomersAsync_MapsCorrectly()
    {
        // Arrange
        int agentId = 2;
        var policy = new CustomerPolicy { CustomerId = 5, PlanId = 1, PolicyNumber = "POL1", Status = PolicyStatus.Active };
        _policyRepoMock.Setup(r => r.GetPoliciesByAgentAsync(agentId))
            .ReturnsAsync(new List<CustomerPolicy> { policy });
        _userRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new User { Id = 5, FullName = "Customer A" });
        _planRepoMock.Setup(r => r.GetPlansWithTiersAsync())
            .ReturnsAsync(new List<PlanDto> { new PlanDto { PlanId = 1, PlanName = "Plan A" } });

        // Act
        var result = await _service.GetMyCustomersAsync(agentId);

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerName.Should().Be("Customer A");
        result[0].PolicyNumber.Should().Be("POL1");
    }
}
