using HartfordInsurance.Domain.Services;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Domain.Tests;

public class CoverageManagerTests
{
    [Fact]
    public void DeductCoverage_ValidAmount_ReturnsRemaining()
    {
        // Act
        var result = CoverageManager.DeductCoverage(10000, 4000);

        // Assert
        result.Should().Be(6000);
    }

    [Fact]
    public void DeductCoverage_ExceedsCoverage_ThrowsException()
    {
        // Act
        Action act = () => CoverageManager.DeductCoverage(5000, 6000);

        // Assert
        act.Should().Throw<Exception>().WithMessage("Coverage exceeded.");
    }

    [Fact]
    public void CanRestore_ConditionsMet_ReturnsTrue()
    {
        // Act
        var result = CoverageManager.CanRestore(true, 1, 2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanRestore_Disabled_ReturnsFalse()
    {
        // Act
        var result = CoverageManager.CanRestore(false, 0, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanRestore_LimitReached_ReturnsFalse()
    {
        // Act
        var result = CoverageManager.CanRestore(true, 2, 2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplyBooster_NoClaim_ReturnsIncreasedCoverage()
    {
        // Arrange
        decimal current = 500000;
        decimal @base = 500000;
        int multiplier = 10;

        // Act
        var result = CoverageManager.ApplyBooster(current, @base, multiplier, true);

        // Assert
        result.Should().Be(550000);
    }

    [Fact]
    public void ApplyBooster_HadClaim_ReturnsCurrentCoverage()
    {
        // Act
        var result = CoverageManager.ApplyBooster(500000, 500000, 10, false);

        // Assert
        result.Should().Be(500000);
    }
}
