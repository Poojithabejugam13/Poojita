using HartfordInsurance.Domain.Models;

namespace HartfordInsurance.Domain.Services;
public static class ClaimValidator
{
    public static void CheckWaitingPeriod(DateTime issueDate, int waitingMonths)
    {
        DateTime eligibleDate = issueDate.AddMonths(waitingMonths);
        if (DateTime.UtcNow < eligibleDate)
        {
            throw new Exception("Waiting period not completed.");
        }
    }
    // Step 1: Validate coverage
    // CheckCoverage is removed as the logic is now to cap the claim rather than reject it completely.
    public static decimal ApplyCoPayment(decimal claimAmount,
decimal coPaymentPercentage)
    {
        if (coPaymentPercentage < 0)
        {
            throw new Exception("Invalid co-payment percentage.");
        }

        decimal deduction = claimAmount * (coPaymentPercentage / 100);

        return claimAmount - deduction;
    }

    public static ClaimResult ProcessClaim(
    decimal claimAmount,
    decimal availableCoverage,
    decimal coPaymentPercentage)
{
    if (claimAmount <= 0)
    {
        throw new Exception("Reject claim if claimAmount <= 0.");
    }

    // Step 3: Eligible claim = min(claimAmount, availableCoverage)
    decimal eligibleClaim = Math.Min(claimAmount, availableCoverage);

    // Step 4: Insurance payment = eligibleClaim - (eligibleClaim * copaymentPercentage/100)
    decimal insurancePayment = eligibleClaim - (eligibleClaim * (coPaymentPercentage / 100m));

    // Step 5: Customer payment = eligibleClaim - insurancePayment
    decimal customerPayment = eligibleClaim - insurancePayment;

    // Step 6: Remaining coverage = availableCoverage - eligibleClaim 
 
decimal remainingCoverage = availableCoverage - eligibleClaim;

    return new ClaimResult
    {
        InsurancePaid = insurancePayment,
        CustomerPaid = customerPayment + (claimAmount > availableCoverage ? claimAmount - availableCoverage : 0),
        RemainingCoverage = remainingCoverage
    };
}
}