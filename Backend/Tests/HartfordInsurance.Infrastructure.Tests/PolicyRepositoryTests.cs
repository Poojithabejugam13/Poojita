using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using HartfordInsurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Infrastructure.Tests;

public class PolicyRepositoryTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_SavePolicy()
    {
        // Arrange
        var db = GetDbContext();
        var repo = new PolicyRepository(db);
        var policy = new CustomerPolicy { PolicyNumber = "POL123", Status = PolicyStatus.PendingApproval };

        // Act
        await repo.AddAsync(policy);

        // Assert
        db.CustomerPolicies.Should().Contain(p => p.PolicyNumber == "POL123");
    }

    [Fact]
    public async Task CountActivePoliciesAsync_Should_FilterByStatus()
    {
        // Arrange
        var db = GetDbContext();
        db.CustomerPolicies.AddRange(new List<CustomerPolicy>
        {
            new CustomerPolicy { CustomerId = 1, Status = PolicyStatus.Active },
            new CustomerPolicy { CustomerId = 1, Status = PolicyStatus.PendingApproval }
        });
        await db.SaveChangesAsync();
        var repo = new PolicyRepository(db);

        // Act
        var count = await repo.CountActivePoliciesAsync(1);

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetTotalCoverageAsync_Should_SumAmounts()
    {
        // Arrange
        var db = GetDbContext();
        db.CustomerPolicies.AddRange(new List<CustomerPolicy>
        {
            new CustomerPolicy { CustomerId = 1, Status = PolicyStatus.Active, RemainingCoverageAmount = 10000 },
            new CustomerPolicy { CustomerId = 1, Status = PolicyStatus.Active, RemainingCoverageAmount = 5000 }
        });
        await db.SaveChangesAsync();
        var repo = new PolicyRepository(db);

        // Act
        var total = await repo.GetTotalCoverageAsync(1);

        // Assert
        total.Should().Be(15000);
    }

    [Fact]
    public async Task ActivatePolicyAsync_Should_ChangeStatus()
    {
        // Arrange
        var db = GetDbContext();
        var policy = new CustomerPolicy { PolicyId = 1, Status = PolicyStatus.PendingApproval };
        db.CustomerPolicies.Add(policy);
        await db.SaveChangesAsync();
        var repo = new PolicyRepository(db);

        // Act
        await repo.ActivatePolicyAsync(1);

        // Assert
        var result = await db.CustomerPolicies.FindAsync(1);
        result!.Status.Should().Be(PolicyStatus.Active);
    }

    [Fact]
    public async Task GetPlanPerformanceAsync_Should_AggregateRevenue()
    {
        // Arrange
        var db = GetDbContext();
        db.InsurancePlans.Add(new InsurancePlan { PlanId = 1, PlanName = "Plan A" });
        db.CustomerPolicies.Add(new CustomerPolicy { PlanId = 1, TotalPremium = 2000 });
        db.CustomerPolicies.Add(new CustomerPolicy { PlanId = 1, TotalPremium = 3000 });
        await db.SaveChangesAsync();
        var repo = new PolicyRepository(db);

        // Act
        var result = await repo.GetPlanPerformanceAsync();

        // Assert
        result.Should().Contain(p => p.PlanName == "Plan A" && p.TotalRevenueGenerated == 5000);
    }
}
