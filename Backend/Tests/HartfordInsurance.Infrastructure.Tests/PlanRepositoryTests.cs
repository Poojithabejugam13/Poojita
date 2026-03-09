using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Infrastructure.Data;
using HartfordInsurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Infrastructure.Tests;

public class PlanRepositoryTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddPlanAsync_Should_AddPlanToDatabase()
    {
        // Arrange
        var context = CreateContext();
        var repository = new PlanRepository(context);
        var plan = new InsurancePlan { PlanName = "Individual Essential Care", PlanType = "Individual" };

        // Act
        var result = await repository.AddPlanAsync(plan);

        // Assert
        result.Should().BeGreaterThan(0);
        var savedPlan = await context.InsurancePlans.FindAsync(result);
        savedPlan.Should().NotBeNull();
        savedPlan!.PlanName.Should().Be("Individual Essential Care");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnPlan_WhenExists()
    {
        // Arrange
        var context = CreateContext();
        var plan = new InsurancePlan { PlanName = "Family FloaterPro", PlanType = "Family" };
        context.InsurancePlans.Add(plan);
        await context.SaveChangesAsync();
        var repository = new PlanRepository(context);

        // Act
        var result = await repository.GetByIdAsync(plan.PlanId);

        // Assert
        result.Should().NotBeNull();
        result!.PlanName.Should().Be("Family FloaterPro");
    }

    [Fact]
    public async Task GetPlansWithTiersAsync_Should_IncludeTiers()
    {
        // Arrange
        var context = CreateContext();
        var plan = new InsurancePlan { PlanName = "Individual Essential Care", PlanType = "Individual" };
        context.InsurancePlans.Add(plan);
        await context.SaveChangesAsync();
        
        var tier = new PlanTier { PlanId = plan.PlanId, TierName = "Shield", BasePremium = 4999 };
        context.PlanTiers.Add(tier);
        await context.SaveChangesAsync();

        var repository = new PlanRepository(context);

        // Act
        var result = await repository.GetPlansWithTiersAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.PlanName == "Individual Essential Care" && p.Tiers.Any(t => t.TierName == "Shield"));
    }

    [Fact]
    public async Task UpdatePlanAsync_Should_ModifyExistingPlan()
    {
        // Arrange
        var context = CreateContext();
        var plan = new InsurancePlan { PlanName = "Old Plan Name", PlanType = "Individual" };
        context.InsurancePlans.Add(plan);
        await context.SaveChangesAsync();
        var repository = new PlanRepository(context);

        // Act
        plan.PlanName = "Updated Plan Name";
        await repository.UpdatePlanAsync(plan);

        // Assert
        var updatedPlan = await context.InsurancePlans.FindAsync(plan.PlanId);
        updatedPlan!.PlanName.Should().Be("Updated Plan Name");
    }

    [Fact]
    public async Task AddTierAsync_Should_AddTier()
    {
        // Arrange
        var context = CreateContext();
        var repository = new PlanRepository(context);
        var tier = new PlanTier { PlanId = 1, TierName = "Apex", BasePremium = 29999 };

        // Act
        await repository.AddTierAsync(tier);

        // Assert
        var savedTier = await context.PlanTiers.FirstOrDefaultAsync(t => t.TierName == "Apex");
        savedTier.Should().NotBeNull();
        savedTier!.BasePremium.Should().Be(29999);
    }
}
