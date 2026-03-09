using HartfordInsurance.Domain.Services;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Domain.Tests;

public class CommissionCalculatorTests
{
    [Fact]
    public void CalculateCommission_ValidInput_ReturnsCorrectAmount()
    {
        // Arrange
        decimal premium = 10000;
        decimal percentage = 15;

        // Act
        var result = CommissionCalculator.CalculateCommission(premium, percentage);

        // Assert
        result.Should().Be(1500);
    }

    [Fact]
    public void CalculateCommission_ZeroPercentage_ReturnsZero()
    {
        // Act
        var result = CommissionCalculator.CalculateCommission(10000, 0);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateCommission_MaxPercentage_ReturnsCorrectAmount()
    {
        // Act
        var result = CommissionCalculator.CalculateCommission(10000, 100);

        // Assert
        result.Should().Be(10000);
    }

    [Fact]
    public void CalculateCommission_NegativePercentage_ThrowsException()
    {
        // Act
        Action act = () => CommissionCalculator.CalculateCommission(10000, -1);

        // Assert
        act.Should().Throw<Exception>().WithMessage("Invalid commission percentage.");
    }

    [Fact]
    public void CalculateCommission_LargeValues_HandlesCorrectly()
    {
        // Arrange
        decimal premium = 1000000;
        decimal percentage = 7.5m;

        // Act
        var result = CommissionCalculator.CalculateCommission(premium, percentage);

        // Assert
        result.Should().Be(75000);
    }
}
