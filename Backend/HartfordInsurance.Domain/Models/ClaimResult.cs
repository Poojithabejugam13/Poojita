namespace HartfordInsurance.Domain.Models;

public class ClaimResult
{
    public decimal InsurancePaid { get; set; }
    public decimal CustomerPaid { get; set; }
    public decimal RemainingCoverage { get; set; }
}
