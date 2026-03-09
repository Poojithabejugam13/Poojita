namespace HartfordInsurance.Domain.Services;

public static class CommissionCalculator
{
    public static decimal CalculateCommission(
        decimal totalPremium,
        decimal commissionPercentage)
    {
        if (commissionPercentage < 0)
        {
            throw new Exception("Invalid commission percentage.");
        }
        return totalPremium * (commissionPercentage / 100);
    }
}