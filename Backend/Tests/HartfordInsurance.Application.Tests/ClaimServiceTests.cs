using HartfordInsurance.Application.DTOs;
using HartfordInsurance.Application.Interfaces;
using HartfordInsurance.Application.Services;
using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using Moq;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Application.Tests;

public class ClaimServiceTests
{
    private readonly Mock<IClaimRepository> _claimRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly ClaimService _service;

    public ClaimServiceTests()
    {
        _claimRepoMock = new Mock<IClaimRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _planRepoMock = new Mock<IPlanRepository>();
        _service = new ClaimService(_claimRepoMock.Object, _policyRepoMock.Object, _userRepoMock.Object, _planRepoMock.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsStatusCounts()
    {
        // Arrange
        _claimRepoMock.Setup(r => r.CountByStatusAsync(ClaimStatus.Pending)).ReturnsAsync(2);
        _claimRepoMock.Setup(r => r.CountByStatusAsync(ClaimStatus.Approved)).ReturnsAsync(5);

        // Act
        var result = await _service.GetDashboardAsync();

        // Assert
        result.PendingClaims.Should().Be(2);
        result.ApprovedClaims.Should().Be(5);
    }

    [Fact]
    public async Task RejectClaimAsync_UpdatesStatusAndReason()
    {
        // Arrange
        var claim = new Claim { ClaimId = 1 };
        _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);

        // Act
        await _service.RejectClaimAsync(1, "Fraudulent");

        // Assert
        claim.Status.Should().Be(ClaimStatus.Rejected);
        claim.RejectionReason.Should().Be("Fraudulent");
        _claimRepoMock.Verify(r => r.UpdateAsync(claim), Times.Once);
    }

    [Fact]
    public async Task ApproveClaimAsync_WithCoPayment_DeductsCorrectAmount()
    {
        // Arrange
        var claim = new Claim { ClaimId = 1, PolicyId = 1, ClaimAmount = 10000 };
        var policy = new CustomerPolicy { PolicyId = 1, TierId = 1, RemainingCoverageAmount = 50000 };
        var tier = new PlanTier { TierId = 1, CoPaymentPercentage = 20 };

        _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);
        _planRepoMock.Setup(r => r.GetTierByIdAsync(1)).ReturnsAsync(tier);

        // Act
        await _service.ApproveClaimAsync(1);

        // Assert
        // Payout = 10000 - 2000 = 8000 (but remaining coverage is deducted by the full eligible amount: 10000)
        policy.RemainingCoverageAmount.Should().Be(40000);
        claim.Status.Should().Be(ClaimStatus.Approved);
    }

    [Fact]
public async Task ApproveClaimAsync_TriggersRestoration_WhenCoverageIsZero()
{
    // Arrange
    var claim = new Claim { ClaimId = 1, PolicyId = 1, ClaimAmount = 10000 };
    var policy = new CustomerPolicy { PolicyId = 1, TierId = 1, RemainingCoverageAmount = 0, AnnualMaxCoverage = 50000, RestoresUsedThisYear = 0 };
    var tier = new PlanTier { TierId = 1, CoverageRestoreEnabled = true, MaxRestoresPerYear = 1, CoPaymentPercentage = 0 };

    _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);
    _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);
    _planRepoMock.Setup(r => r.GetTierByIdAsync(1)).ReturnsAsync(tier);

    // Act
    await _service.ApproveClaimAsync(1);

    // Assert
    // Restore triggers: Coverage becomes 50000
    // Claim 10000 processed. Remaining = 50000 - 10000 = 40000
    policy.RemainingCoverageAmount.Should().Be(40000);
    policy.RestoresUsedThisYear.Should().Be(1);
}

    [Fact]
    public async Task GetMyCustomersAsync_MapsDetailsCorrectly()
    {
        // Arrange
        int officerId = 4;
        var policy = new CustomerPolicy { CustomerId = 5, PlanId = 1, PolicyNumber = "POL1", Status = PolicyStatus.Active };
        _policyRepoMock.Setup(r => r.GetPoliciesByClaimsOfficerAsync(officerId)).ReturnsAsync(new List<CustomerPolicy> { policy });
        _userRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new User { Id = 5, FullName = "Creed" });
        _planRepoMock.Setup(r => r.GetPlansWithTiersAsync()).ReturnsAsync(new List<PlanDto> { new PlanDto { PlanId = 1, PlanName = "Plan A" } });
        _claimRepoMock.Setup(r => r.GetClaimsByPolicyAsync(It.IsAny<int>())).ReturnsAsync(new List<Claim>());

        // Act
        var result = await _service.GetMyCustomersAsync(officerId);

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerName.Should().Be("Creed");
    }
}
