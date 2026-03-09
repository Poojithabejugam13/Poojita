using HartfordInsurance.Domain.Entities;
using HartfordInsurance.Domain.Enums;
using HartfordInsurance.Infrastructure.Data;
using HartfordInsurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Infrastructure.Tests;

public class UserRepositoryTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_PersistUser()
    {
        // Arrange
        var db = GetDbContext();
        var repo = new UserRepository(db);
        var user = new User { FullName = "Test User", Email = "test@user.com", Role = Role.Customer };

        // Act
        await repo.AddAsync(user);

        // Assert
        var result = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@user.com");
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnUser_WhenExists()
    {
        // Arrange
        var db = GetDbContext();
        db.Users.Add(new User { Email = "found@user.com", FullName = "Found" });
        await db.SaveChangesAsync();
        var repo = new UserRepository(db);

        // Act
        var result = await repo.GetByEmailAsync("found@user.com");

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Found");
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyUser()
    {
        // Arrange
        var db = GetDbContext();
        var user = new User { Id = 1, FullName = "Old Name", Email = "old@user.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var repo = new UserRepository(db);

        // Act
        user.FullName = "New Name";
        await repo.UpdateAsync(user);

        // Assert
        var result = await db.Users.FindAsync(1);
        result!.FullName.Should().Be("New Name");
    }

    [Fact]
    public async Task CountByRoleAsync_Should_ReturnCorrectCount()
    {
        // Arrange
        var db = GetDbContext();
        db.Users.AddRange(new List<User>
        {
            new User { Role = Role.Agent },
            new User { Role = Role.Agent },
            new User { Role = Role.Customer }
        });
        await db.SaveChangesAsync();
        var repo = new UserRepository(db);

        // Act
        var count = await repo.CountByRoleAsync(Role.Agent);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetAgentPerformanceAsync_Should_CalculateStats()
    {
        // Arrange
        var db = GetDbContext();
        var agent = new User { Id = 1, FullName = "Agent Smith", Role = Role.Agent };
        db.Users.Add(agent);
        db.CustomerPolicies.Add(new CustomerPolicy { AgentId = 1, CommissionAmount = 500 });
        db.CustomerPolicies.Add(new CustomerPolicy { AgentId = 1, CommissionAmount = 300 });
        await db.SaveChangesAsync();
        var repo = new UserRepository(db);

        // Act
        var result = await repo.GetAgentPerformanceAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].PoliciesSold.Should().Be(2);
        result[0].TotalCommissionEarned.Should().Be(800);
    }
}
