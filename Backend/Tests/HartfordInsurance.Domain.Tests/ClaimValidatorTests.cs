using HartfordInsurance.Domain.Services;
using FluentAssertions;
using Xunit;

namespace HartfordInsurance.Domain.Tests;

public class ClaimValidatorTests
{
    [Fact]
    public void CheckWaitingPeriod_Completed_DoesNotThrow()
    {
        // Arrange
        var issueDate = DateTime.UtcNow.AddMonths(-25);
        int waitingMonths = 24;

        // Act
        Action act = () => ClaimValidator.CheckWaitingPeriod(issueDate, waitingMonths);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CheckWaitingPeriod_NotCompleted_ThrowsException()
    {
        // Arrange
        var issueDate = DateTime.UtcNow.AddMonths(-10);
        int waitingMonths = 12;

        // Act
        Action act = () => ClaimValidator.CheckWaitingPeriod(issueDate, waitingMonths);

        // Assert
        act.Should().Throw<Exception>().WithMessage("Waiting period not completed.");
    }



    [Theory]
    [InlineData(100000, 500000, 10, 90000, 10000, 400000)] // Case 2
    [InlineData(350000, 200000, 10, 180000, 170000, 0)]    // Case 3 (Customer pays 20k copay + 150k uncovered)
    [InlineData(100000, 20000, 10, 18000, 82000, 0)]       // Case 4 (Customer pays 2k copay + 80k uncovered)
    [InlineData(100000, 500000, 0, 100000, 0, 400000)]     // Case 9
    [InlineData(200000, 200000, 10, 180000, 20000, 0)]     // Case 12
    public void ProcessClaim_ValidCases_CalculatesCorrectly(
        decimal claimAmount, decimal availableCoverage, decimal coPaymentPercentage,
        decimal expectedInsurancePaid, decimal expectedCustomerPaid, decimal expectedRemaining)
    {
        // Act
        var result = ClaimValidator.ProcessClaim(claimAmount, availableCoverage, coPaymentPercentage);

        // Assert
        result.InsurancePaid.Should().Be(expectedInsurancePaid);
        result.CustomerPaid.Should().Be(expectedCustomerPaid);
        result.RemainingCoverage.Should().Be(expectedRemaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void ProcessClaim_InvalidAmount_ThrowsException(decimal invalidAmount)
    {
        Action act = () => ClaimValidator.ProcessClaim(invalidAmount, 100000, 10);
        act.Should().Throw<Exception>().WithMessage("Reject claim if claimAmount <= 0.");
    }
}
