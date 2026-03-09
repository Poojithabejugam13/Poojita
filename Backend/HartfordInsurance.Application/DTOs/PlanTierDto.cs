namespace HartfordInsurance.Application.DTOs;
public class PlanTierDto
{
    public int PlanId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public decimal BasePremium { get; set; }
    public decimal CoverageLimit { get; set; }
    public bool AgeLockProtection { get; set; }
    public bool CoverageRestoreEnabled { get; set; }
    public int MaxRestoresPerYear { get; set; }
    public int BoosterMultiplier { get; set; }
    public int PreExistingDiseaseWaitingMonths { get; set; }
    public decimal CoPaymentPercentage { get; set; }
    public decimal CommissionPercentage { get; set; }
}
