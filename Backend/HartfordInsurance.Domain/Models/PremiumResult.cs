namespace HartfordInsurance.Domain.Models;
public class PremiumResult
{
    public decimal TotalPremium { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal AgeLoading { get; set; }
    public decimal RiskLoading { get; set; }
}