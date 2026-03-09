using System.ComponentModel.DataAnnotations;
namespace HartfordInsurance.Domain.Entities;

public class PlanTier
{
    [Key]
    public int TierId { get; set; }
    public int PlanId { get; set; }   
    public string TierName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // Pricing
    public decimal BasePremium { get; set; }
    public decimal BaseCoverageAmount { get; set; }
    // Premium Protection
    public bool AgeLockProtection { get; set; }
    // Coverage Restore
    public bool CoverageRestoreEnabled { get; set; }
    public int MaxRestoresPerYear { get; set; }
    // Coverage Growth
    public int BoosterMultiplier { get; set; }
    // Claim Rules
    public int PreExistingDiseaseWaitingMonths { get; set; }
    public decimal CoPaymentPercentage { get; set; }
    public decimal CommissionPercentage { get; set; } = 0;
}