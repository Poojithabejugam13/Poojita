using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using HartfordInsurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Infrastructure.Tests;

public class ClaimRepositoryTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_PersistClaim()
    {
        // Arrange
        var db = GetDbContext();
        var repo = new ClaimRepository(db);
        var claim = new Claim { ClaimReason = "Accident", ClaimAmount = 5000 };

        // Act
        await repo.AddAsync(claim);

        // Assert
        db.Claims.Should().Contain(c => c.ClaimReason == "Accident");
    }

    [Fact]
    public async Task CountPendingClaimsAsync_Should_FilterByCustomerAndStatus()
    {
        // Arrange
        var db = GetDbContext();
        db.CustomerPolicies.Add(new CustomerPolicy { PolicyId = 1, CustomerId = 10 });
        db.Claims.Add(new Claim { PolicyId = 1, Status = ClaimStatus.Pending });
        db.Claims.Add(new Claim { PolicyId = 1, Status = ClaimStatus.Approved });
        await db.SaveChangesAsync();
        var repo = new ClaimRepository(db);

        // Act
        var count = await repo.CountPendingClaimsAsync(10);

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetPendingClaimDtosAsync_Should_ReturnProjectedData()
    {
        // Arrange
        var db = GetDbContext();
        db.InsurancePlans.Add(new InsurancePlan { PlanId = 1, PlanName = "Test Plan", Description = "Desc", PlanType = "Individual" });
        db.PlanTiers.Add(new PlanTier { TierId = 101, PlanId = 1, TierName = "Shield", BasePremium = 1000, BaseCoverageAmount = 100000 });
        db.CustomerPolicies.Add(new CustomerPolicy { PolicyId = 1, PlanId = 1, TierId = 101, CustomerId = 1 });
        db.Claims.Add(new Claim { ClaimId = 1, PolicyId = 1, Status = ClaimStatus.Pending, ClaimAmount = 2500, ClaimReason = "Fever" });
        await db.SaveChangesAsync();
        var repo = new ClaimRepository(db);

        // Act
        var result = await repo.GetPendingClaimDtosAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].ClaimAmount.Should().Be(2500);
        result[0].Reason.Should().Be("Fever");
    }

    [Fact]
    public async Task GetClaimsByCustomerAsync_Should_JoinCorrectly()
    {
        // Arrange
        var db = GetDbContext();
        db.InsurancePlans.Add(new InsurancePlan { PlanId = 1, PlanName = "Plan X", PlanType = "Individual", Description = "Desc" });
        db.PlanTiers.Add(new PlanTier { TierId = 101, PlanId = 1, TierName = "Tier Y" });
        db.CustomerPolicies.Add(new CustomerPolicy { PolicyId = 1, PlanId = 1, TierId = 101, CustomerId = 5 });
        db.Claims.Add(new Claim { PolicyId = 1, ClaimReason = "Reason A" });
        await db.SaveChangesAsync();
        var repo = new ClaimRepository(db);

        // Act
        var result = await repo.GetClaimsByCustomerAsync(5);

        // Assert
        result.Should().HaveCount(1);
        result[0].Reason.Should().Be("Reason A");
    }

    [Fact]
    public async Task AddDocumentAsync_Should_PersistDocument()
    {
        // Arrange
        var db = GetDbContext();
        var repo = new ClaimRepository(db);
        var doc = new Document { DocumentType = "MedicalReport", FilePath = "/docs/report.pdf" };

        // Act
        await repo.AddDocumentAsync(doc);

        // Assert
        db.Documents.Should().Contain(d => d.DocumentType == "MedicalReport");
    }
}
